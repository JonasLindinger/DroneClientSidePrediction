using LindoNoxStudio.Network.Input;
using LindoNoxStudio.Network.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Player
{
    [RequireComponent(typeof(PlayerNetworkedObject))]
    public class PlayerStateSyncronisation : NetworkBehaviour
    {
        #if Client
        // References
        private PlayerNetworkedObject _playerNetworkedObject;
        #elif Server
        
        // The Ammount of ClientInputStates and PlayerStates we save
        private const int StateBufferSize = 128; // 8 bit | 1 byte
        
        // Circular Buffers
        private ClientInputState[] _inputStates = new ClientInputState[StateBufferSize];
        private PlayerState[] _playerStates = new PlayerState[StateBufferSize];
        
        #endif
        
        // Values
        private uint _latestStateTick;
        
        // References
        private PlayerController _playerController;

        public override void OnNetworkSpawn()
        {
            // Referencing
            _playerController = GetComponent<PlayerController>();
            #if Client
            _playerNetworkedObject = GetComponent<PlayerNetworkedObject>();
            #endif
        }


        #if Client
        
        /// <summary>
        /// Compairs the ServerState with the predicted ClientState
        /// If they match, we do nothing.
        /// If they don't we reconcile.
        /// </summary>
        /// <param name="serverState"></param>
        /// <param name="inputUsedForNextTick"></param>
        private void HandleReconciliation(PlayerState serverState, ClientInputState inputUsedForNextTick)
        {
            // Getting player state
            PlayerState clientState = _playerNetworkedObject.GetSnapshot(serverState.Tick);
            
            // Validating player state
            if (clientState == null)
                return;
            else if (clientState.Tick != serverState.Tick)
                return;
            
            // Compairing player state
            if (Vector3.Distance(clientState.Position, serverState.Position) >= 0.001f)
            {
                Reconcile(serverState, inputUsedForNextTick);
            }
            else if (Vector3.Distance(clientState.Velocity, serverState.Velocity) >= 0.001f)
            {
                Reconcile(serverState, inputUsedForNextTick);
            }
            else 
                // If there is nothing to reconcile, we take a snapshot???
                _playerNetworkedObject.TakeSnapshot(serverState.Tick);
        }

        /// <summary>
        /// We apply the server State, predict all new ticks again and save the states
        /// </summary>
        /// <param name="correctState"></param>
        /// <param name="inputUsedForNextTick"></param>
        private void Reconcile(PlayerState correctState, ClientInputState inputUsedForNextTick)
        {
            // Logging mistake
            Debug.LogWarning("Prediction was not correct. ");
            
            // Save the state
            NetworkedObject.Rollback(correctState.Tick);
            
            // Applying correct state
            _playerNetworkedObject.ApplySnapshot(correctState);
            _playerNetworkedObject.TakeSnapshot(correctState.Tick);
            
            // Use the input to predict the next states
            _playerController.OnInput(inputUsedForNextTick);
            
            // For each new tick, we predict again.
            for (uint tick = correctState.Tick + 1; tick < SimulationManager.CurrentTick + 1; tick++)
            {
                SimulationManager.HandlePhysicsTick(tick, true);
            }
        }
        
        #elif Server
        
        public void SaveState(uint tick, ClientInputState input)
        {
            // Saving last state to send to the client later
            PlayerState state = _playerController.GetState(tick);
            _playerStates[tick % StateBufferSize] = state;
            _latestStateTick = tick;
        }
        
        public void SendState() 
        {
            PlayerState stateToSend = _playerStates[_latestStateTick % StateBufferSize];
            ClientInputState inputToSend = _inputStates[_latestStateTick % StateBufferSize];
            OnServerStateRPC(stateToSend, inputToSend);
        } 
        
        #endif
        
        /// <summary>
        /// Remote Procedural Call.
        /// The Server sends the player state to every client
        /// </summary>
        /// <param name="playerState"></param>
        /// <param name="inputUsedForNextTick"></param>
        [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Reliable)]
        private void OnServerStateRPC(PlayerState playerState, ClientInputState inputUsedForNextTick)
        {
            #if Client
            // Warning: Don't run this RPC unreliable, without changing this code down here!!!!!!!!!!!!!!!!!!!!!!!
            if (!IsOwner)
            {
                // If this is not our player, we apply the snapshot to see the changes
                _playerNetworkedObject.ApplySnapshot(playerState);
                _playerNetworkedObject.TakeSnapshot(playerState.Tick);
            }
            else
            {
                // If this is our player, we compaire the state with our predicted state.
                _latestStateTick = playerState.Tick;
            
                HandleReconciliation(playerState, inputUsedForNextTick);
            }
            #endif
        }
    }
}