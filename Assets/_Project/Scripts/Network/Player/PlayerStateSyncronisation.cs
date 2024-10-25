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
        private PlayerNetworkedObject _playerNetworkedObject;
        #elif Server
        
        private const int StateBufferSize = 128; // 8 bit | 1 byte
        
        private ClientInputState[] _inputStates = new ClientInputState[StateBufferSize];
        private PlayerState[] _playerStates = new PlayerState[StateBufferSize];
        
        #endif
        
        private uint _latestStateTick;
        
        private PlayerController _playerController;

        public override void OnNetworkSpawn()
        {
            _playerController = GetComponent<PlayerController>();
            #if Client
            
            _playerNetworkedObject = GetComponent<PlayerNetworkedObject>();
            
            #endif
        }


        #if Client
        
        private void HandleReconciliation(PlayerState serverState, ClientInputState inputUsedForNextTick)
        {
            PlayerState clientState = _playerNetworkedObject.GetSnapshot(serverState.Tick);
            
            if (clientState == null)
                return;
            else if (clientState.Tick != serverState.Tick)
                return;
            
            // Setting rotation
            _playerNetworkedObject.OverrideRotation(serverState.Tick, serverState.Rotation);
            _playerController.ApplyVisualRotation(serverState.Rotation.x, serverState.Rotation.y);
            
            if (Vector3.Distance(clientState.Position, serverState.Position) >= 0.001f)
            {
                Reconcile(serverState, inputUsedForNextTick);
            }
            else if (Vector3.Distance(clientState.Velocity, serverState.Velocity) >= 0.001f)
            {
                Reconcile(serverState, inputUsedForNextTick);
            }
            else 
            {
                _playerNetworkedObject.TakeSnapshot(serverState.Tick);
            }
        }

        private void Reconcile(PlayerState correctState, ClientInputState inputUsedForNextTick)
        {
            Debug.LogWarning("Prediction was not correct. ");
            // Save the state
            NetworkedObject.Rollback(correctState.Tick);
            
            // Applying correct state
            _playerNetworkedObject.ApplySnapshot(correctState);
            _playerNetworkedObject.TakeSnapshot(correctState.Tick);
            
            // Use the input to predict the next states
            _playerController.OnInput(inputUsedForNextTick);
            
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
        
        [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Reliable)]
        private void OnServerStateRPC(PlayerState playerState, ClientInputState inputUsedForNextTick)
        {
            #if Client
            // Warning: Don't run this RPC unreliable, without changing this code down here!!!!!!!!!!!!!!!!!!!!!!!
            if (!IsOwner)
            {
                _playerNetworkedObject.ApplySnapshot(playerState);
                _playerNetworkedObject.TakeSnapshot(playerState.Tick);
            }
            else
            {
                _latestStateTick = playerState.Tick;
            
                HandleReconciliation(playerState, inputUsedForNextTick);
            }
            #endif
        }
    }
}