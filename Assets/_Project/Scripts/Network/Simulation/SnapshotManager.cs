using System.Collections.Generic;
using LindoNoxStudio.Network.Ball;
using LindoNoxStudio.Network.Player;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

namespace LindoNoxStudio.Network.Simulation
{
    public static class SnapshotManager
    {
        private static Dictionary<ulong, NetworkedObject> _networkedObjects  = new Dictionary<ulong, NetworkedObject>(); // NetworkObjectId | PredictionObject
        
        private const int GameStateBufferSize = 128;
        private static GameState[] _gameStates = new GameState[GameStateBufferSize];

        private static uint _latestSavedGameStateTick;
        
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
        public static void TakeSnapshot(uint tick)
        {
            GameState currentGameState = GetCurrentState(tick);
            _gameStates[(int)tick % GameStateBufferSize] = currentGameState;
            _latestSavedGameStateTick = tick;
        }

        #if Server
        /// <summary>
        /// Returns the latest saved snapshot
        /// </summary>
        /// <returns></returns>
        public static GameState GetLatestSnapshot()
        {
            return _gameStates[(int)_latestSavedGameStateTick % GameStateBufferSize];
        }
        #endif
        
        /// <summary>
        /// Returns the current GameState.
        /// </summary>
        /// <param name="tick">Current Tick</param>
        private static GameState GetCurrentState(uint tick)
        {
            GameState currentGameState = new GameState
            {
                Tick = tick
            };
    
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

                    DebugDrawCircle(playerData.playerState.Position, 0.5f, Color.green);
                    DebugDrawCircle(playerData.predictedPlayerState.Position, 0.5f, Color.red);
                    
                    return CheckForReconciliation(playerData.playerState, playerData.predictedPlayerState);
                    break;
            }

            return true;
        }
        
        private static void DebugDrawCircle(Vector3 center, float radius, Color color, int segments = 36)
        {
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float nextAngle = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 start = new Vector3(Mathf.Cos(angle), 1, Mathf.Sin(angle)) * radius + center;
                Vector3 end = new Vector3(Mathf.Cos(nextAngle), 1, Mathf.Sin(nextAngle)) * radius + center;

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
            if (Vector3.Distance(ballState.Position, predictedBallState.Position) > 0.001f)
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
        public static void ApplyState(uint tick, ulong networkId, IState state, bool isLocalPlayer = false)
        {
            if (!_networkedObjects.TryGetValue(networkId, out NetworkedObject networkedObject) || networkedObject == null)
            {
                Debug.LogWarning($"Networked object with ID {networkId} not found!");
                return;
            }
    
            // Check for reconciliation requirement
            if (CheckForReconciliation(tick, networkId, state))
            {
                if (isLocalPlayer)
                {
                    Debug.LogWarning("Local player prediction was incorrect; reconciling with server state.");
                    networkedObject.ApplyState(state); // Immediate for local player
                }
                else
                {
                    networkedObject.ApplyState(state); // Lerp the position
                    networkedObject.ApplyNecessaryThings(state); // Apply any required adjustments
                }
            }
            else if (!isLocalPlayer)
            {
                networkedObject.ApplyNecessaryThings(state); // Minor corrections without full reconciliation
            }
        }
        
        #endif
    }
}