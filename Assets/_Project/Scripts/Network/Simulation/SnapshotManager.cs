using System.Collections.Generic;
using LindoNoxStudio.Network.Ball;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public static class SnapshotManager
    {
        private static Dictionary<ulong, NetworkedObject> _networkedObjects  = new Dictionary<ulong, NetworkedObject>(); // NetworkObjectId | PredictionObject
        
        private const int GameStateBufferSize = 128;
        private static GameState[] _gameStates = new GameState[GameStateBufferSize];
        
        /// <summary>
        /// Every Networked Object registered will be included in the GameState.
        /// It wouldn't make to Unregister Objects so we don't have a method for that!
        /// </summary>
        /// <param name="id">NetworkId</param>
        /// <param name="networkedObject"></param>
        public static void RegisterNetworkedObject(ulong id, NetworkedObject networkedObject)
        {
            _networkedObjects.Add(id, networkedObject);
        }
        
        /// <summary>
        /// Saves the current GameState.
        /// </summary>
        /// <param name="tick">Current Tick</param>
        public static GameState TakeSnapshot(uint tick)
        {
            GameState currentGameState = GetCurrentState(tick);

            _gameStates[(int)tick % GameStateBufferSize] = currentGameState;
            return currentGameState;
        }

        /// <summary>
        /// Returns the current GameState.
        /// </summary>
        /// <param name="tick">Current Tick</param>
        private static GameState GetCurrentState(uint tick)
        {
            GameState currentGameState = new GameState();
            currentGameState.Tick = tick;
            
            foreach (var kvp in _networkedObjects)
            {
                ulong networkId = kvp.Key;
                NetworkedObject networkedObject = kvp.Value;

                IState state = networkedObject.GetCurrentState();
                
                currentGameState.States.Add(networkId, state);
            }
            
            return currentGameState;
        }
        
        #if Client

        public static bool CheckForReconciliation(uint tick, ulong networkId, IState state)
        {
            switch (state.GetStateType())
            {
                case StateType.Ball:
                    var ballData = GetBallStates(tick, networkId, state);
                    // Break if the data isn't valid
                    if (!ballData.isValid)
                    {
                        Debug.LogWarning("Not Valid Data!");
                        break;
                    }
                    return CheckForReconciliation(ballData.ballState, ballData.predictedBallState);
                    break;
                case StateType.Player:
                    var playerData = GetPlayerStates(tick, networkId, state);
                    Debug.Log("IsValid: " + playerData.isValid);
                    // Break if the data isn't valid
                    if (!playerData.isValid)
                    {
                        Debug.LogWarning("Not Valid Data!");
                        break;
                    }
                    return CheckForReconciliation(playerData.playerState, playerData.predictedPlayerState);
                    break;
            }

            return true;
        }

        #region Player State

        /// <summary>
        /// Returns the predicted Player State and the given Player State, if it is a Player State. It also returns  if the values are valid or not.
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="networkId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private static (bool isValid, PlayerState playerState, PlayerState predictedPlayerState) GetPlayerStates(uint tick, ulong networkId, IState state)
        {
            GameState predictedGameState = _gameStates[tick % GameStateBufferSize];

            // Check if predictedGameState is null
            if (predictedGameState == null)
                // Something went wrong, so we reconcile for savety
                return (false, new PlayerState(), new PlayerState());

            // Check if we have the right GameState
            if (predictedGameState.Tick != tick)
                // Something went wrong, so we reconcile for savety
                return (false, new PlayerState(), new PlayerState());
            
            // Check if we have the networkedObject
            if (!predictedGameState.States.ContainsKey(networkId)) 
                // Something went wrong, so we reconcile for savety
                return (false, new PlayerState(), new PlayerState());
            
            IState predictedGeneralState = predictedGameState.States[networkId];
            
            // Check if the predicted state is a Player
            if (!(predictedGeneralState is PlayerState predictedPlayerState))
                // Something went wrong, so we reconcile for savety
                return (false, new PlayerState(), new PlayerState());
            
            // Check if the given state is a Player
            if (!(state is PlayerState playerState))
                // Something went wrong, so we reconcile for savety
                return (false, new PlayerState(), new PlayerState());
            
            return (true, playerState, predictedPlayerState);
        }

        /// <summary>
        /// Returns if we should Reconcile or not.
        /// </summary>
        /// <param name="playerState"></param>
        /// <param name="predictedPlayerState"></param>
        /// <returns></returns>
        private static bool CheckForReconciliation(PlayerState playerState, PlayerState predictedPlayerState)
        {
            // Check for Position error
            if (Vector3.Distance(playerState.Position, predictedPlayerState.Position) < 0.001f)
                return true;
            else return false;
        }

        #endregion
        
        #region Ball State

        /// <summary>
        /// Returns the predicted Ball State and the given Ball State, if it is a Ball State. It also returns if the values are valid or not.
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="networkId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private static (bool isValid, BallState ballState, BallState predictedBallState) GetBallStates(uint tick, ulong networkId, IState state)
        {
            GameState predictedGameState = _gameStates[tick % GameStateBufferSize];

            // Check if predictedGameState is null
            if (predictedGameState == null)
                // Something went wrong, so we reconcile for savety
                return (false, new BallState(), new BallState());

            // Check if we have the right GameState
            if (predictedGameState.Tick != tick)
                // Something went wrong, so we reconcile for savety
                return (false, new BallState(), new BallState());
            
            // Check if we have the networkedObject
            if (!predictedGameState.States.ContainsKey(networkId)) 
                // Something went wrong, so we reconcile for savety
                return (false, new BallState(), new BallState());
            
            IState predictedGeneralState = predictedGameState.States[networkId];
            
            // Check if the predicted state is a Player
            if (!(predictedGeneralState is BallState predictedBallState))
                // Something went wrong, so we reconcile for savety
                return (false, new BallState(), new BallState());
            
            // Check if the given state is a Player
            if (!(state is BallState ballState))
                // Something went wrong, so we reconcile for savety
                return (false, new BallState(), new BallState());
            
            return (true, ballState, predictedBallState);
        }

        /// <summary>
        /// Returns if we should Reconcile or not.
        /// </summary>
        /// <param name="ballState"></param>
        /// <param name="predictedBallState"></param>
        /// <returns></returns>
        private static bool CheckForReconciliation(BallState ballState, BallState predictedBallState)
        {
            // Check for Position error
            if (Vector3.Distance(ballState.Position, predictedBallState.Position) < 0.001f)
                return true;
            else return false;
        }

        #endregion
        
        /// <summary>
        /// Reconciles every object exept the local player
        /// </summary>
        /// <param name="gameState"></param>
        public static void Reconcile(GameState gameState)
        {
            // Todo: Do Reconciliation method
        }

        /// <summary>
        /// Applys the state on the object with the corresponding network Id
        /// </summary>
        /// <param name="networkId">Object'S NetworkId</param>
        /// <param name="state"></param>
        public static void ApplyState(uint tick, ulong networkId, IState state)
        {
            NetworkedObject networkedObject = _networkedObjects[networkId];

            // Check for null reference
            if (networkedObject == null)
            {
                Debug.LogWarning("Something went wrong!");
                return;
            }

            networkedObject.ApplyState(state);
            CheckForReconciliation(tick, networkId, state);
            return;
            
            if (CheckForReconciliation(tick, networkId, state))
            {
                networkedObject.ApplyState(state);
            }
            else
            {
                networkedObject.ApplyNecessaryThings(state);
            }
        }

        #endif
    }
}