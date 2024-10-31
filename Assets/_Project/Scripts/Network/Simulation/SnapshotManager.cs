using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LindoNoxStudio.Network.Ball;
using LindoNoxStudio.Network.Input;
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
        
        private const int GameStateBufferSize = 1028;
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
        public static GameState GetCurrentState(uint tick)
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

        /// <summary>
        /// Applys the state on the object with the corresponding network Id
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="networkId"></param>
        /// <param name="state"></param>
        /// <param name="isLocal"></param>
        /// <returns>Return's if the prediction was wrong</returns>
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
        }

        #elif Server
        /// <summary>
        /// Returns the newest GameState
        /// </summary>
        public static GameState GetLatestGameState()
        {
            return _gameStates[(int)_latestGameStateTick % GameStateBufferSize];
        }
        #endif
    }
}