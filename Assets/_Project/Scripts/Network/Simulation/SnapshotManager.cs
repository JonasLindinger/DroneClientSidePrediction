using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        
        #if Server
        private static uint _latestGameStateTick;
        #endif
        
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

        #if Server
        /// <summary>
        /// Returns the newest GameState
        /// </summary>
        public static GameState GetLatestGameState()
        {
            return _gameStates[(int)_latestGameStateTick % GameStateBufferSize];
        }
        #endif
        
        /// <summary>
        /// Saves the current GameState.
        /// </summary>
        /// <param name="tick">Current Tick</param>
        public static void TakeSnapshot(uint tick)
        {
            GameState currentGameState = GetCurrentState(tick);

            _gameStates[(int)tick % GameStateBufferSize] = currentGameState;
            #if Server
            _latestGameStateTick = tick;
            #endif
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
                    // Break if the data isn't valid
                    if (!playerData.isValid)
                    {
                        Debug.LogWarning("Not Valid Data!");
                        break;
                    }
                    // Debugging position
                    DebugDrawCircle(playerData.playerState.Position,0.5f, Color.grey);
                    DebugDrawCircle(playerData.predictedPlayerState.Position,0.5f, Color.green);
                    
                    return CheckForReconciliation(playerData.playerState, playerData.predictedPlayerState);
                    break;
            }

            return true;
        }
        
        // Todo: Remove this in final build
        /// <summary>
        /// Debugging method
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="segments"></param>
        private static void DebugDrawCircle(Vector3 center, float radius, Color color, int segments = 36)
        {
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float nextAngle = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 start = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius + center;
                Vector3 end = new Vector3(Mathf.Cos(nextAngle), 0, Mathf.Sin(nextAngle)) * radius + center;

                Debug.DrawLine(start, end, color);
            }
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
            if (Vector3.Distance(playerState.Position, predictedPlayerState.Position) > 0.001f)
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
        public static void ApplyState(uint tick, ulong networkId, IState state, bool isLocal)
        {
            NetworkedObject networkedObject = _networkedObjects[networkId];

            // Check for null reference
            if (networkedObject == null)
            {
                Debug.LogWarning("Something went wrong!");
                return;
            }
            
            if (CheckForReconciliation(tick, networkId, state))
            {
                if (isLocal) 
                    Debug.LogWarning("Local player prediction was wrong!");
                else 
                    Debug.Log("Remote player prediction was wrong!");
                networkedObject.ApplyState(state);
            }
            // If we predicted correct. We apply the necessary things like velocity for the next prediction.
            // But if this is the local player. We don't do that because we have the input of the client
            else if (isLocal)
            {
                if (isLocal) 
                    Debug.Log("Local player prediction was right!");
                else 
                    Debug.Log("Remote player prediction was right!");
                networkedObject.ApplyNecessaryThings(state);
            }
        }
        
        /// <summary>
        /// Applys the state on the object with the corresponding network Id
        /// </summary>
        /// <param name="networkId">Object'S NetworkId</param>
        /// <param name="state"></param>
        public static void ApplyState(uint tick, ulong networkId)
        {
            NetworkedObject networkedObject = _networkedObjects[networkId];
            if (_gameStates[tick % GameStateBufferSize].Tick != tick)
            {
                Debug.LogWarning("Something went wrong!");
                return;
            }
            
            networkedObject.ApplyState(_gameStates[tick % GameStateBufferSize].States[networkId]);
        }

        #endif
    }
}