using System;
using System.Collections.Generic;
using LindoNoxStudio.Network.Player;
using LindoNoxStudio.Network.Simulation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LindoNoxStudio.Network.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class ClientInput : NetworkBehaviour
    {
        private const int InputBufferSize = 128; // 8 bit | 1 byte
        
        #if Client
        private ClientInputState[] _localClientInputStates = new ClientInputState[InputBufferSize];

        private uint _tickWeFirstStartedSavingInputs;
        
        private PlayerInput _playerInput;
        
        #elif Server
        private ClientInputState[] _clientInputStates = new ClientInputState[InputBufferSize];
        
        #endif

        [HideInInspector] public int _bufferSize = 3;
        
        public override void OnNetworkSpawn()
        {
            #if Client
            
            _playerInput = GetComponent<PlayerInput>();
            
            #endif
        }

        #if Client
        
        public void SaveInput(uint tick)
        {
            // Getting the current input state
            ClientInputState inputForThisTick = GetClientInputState(tick, true);
            
            // Saving the input in the dictionary
            _localClientInputStates[tick % InputBufferSize] = inputForThisTick;

            // Setting the variable for later use
            if (_tickWeFirstStartedSavingInputs == 0)
                _tickWeFirstStartedSavingInputs = tick;
        }

        public void SendInputs()
        {
            // If we haven't saved any inputs yet, then we don't have anything to send
            if (_tickWeFirstStartedSavingInputs == 0) return;
            
            // Sending the input with some last inputs to the server
            // Todo: Dynamic input array size (currently it's static on 15)
            OnClientInputsRPC(GetInputsToSend(15));
        }

        public ClientInputState GetClientInputState(uint tick, bool isCurrentTick = false)
        {
            ClientInputState clientInputState = new ClientInputState();
            if (isCurrentTick)
            {
                // Getting Inputs
                Vector2 Cycle = _playerInput.actions["Cycle"].ReadValue<Vector2>();
                float Pedals = _playerInput.actions["Pedals"].ReadValue<float>();
                float Throttle = _playerInput.actions["Throttle"].ReadValue<float>();
                
                // Setting rotation
                try
                {
                    PlayerController playerController = NetworkPlayer.LocalNetworkPlayer._playerController;
                    clientInputState.SetUp(tick, Cycle, Pedals, Throttle, playerController.yaw);
                }
                catch (NullReferenceException e)
                {
                    clientInputState.SetUp(tick, Cycle, Pedals, Throttle, 0);
                }
            }
            else
            {
                clientInputState = _localClientInputStates[tick % InputBufferSize];
            }

            return clientInputState;
        }
        
        private ClientInputState[] GetInputsToSend(int requestedSize)
        {
            // Calculate the current input count
            // If the tick is 15 and we just started saving the inputs at tick 15, then the current input count is 1 since 15 - (15 - 1) = 1
            int currentInputCount = Mathf.RoundToInt(SimulationManager.CurrentTick - (_tickWeFirstStartedSavingInputs - 1));
    
            // Adjust the size to return the minimum of requestedSize or available inputs
            int sizeToReturn = Math.Min(requestedSize, currentInputCount);

            // Prepare the array to hold the inputs
            ClientInputState[] inputs = new ClientInputState[sizeToReturn];

            // Calculate the starting index in the circular buffer
            int startIndex = (int)(SimulationManager.CurrentTick % InputBufferSize);

            // Fetch the most recent inputs, going backwards through the circular buffer
            for (int i = 0; i < sizeToReturn; i++)
            {
                // Calculate the index in the circular buffer, wrapping around if necessary
                int currentIndex = (startIndex - i + InputBufferSize) % InputBufferSize;

                // Retrieve the input from the circular buffer array
                inputs[i] = _localClientInputStates[currentIndex];
            }

            return inputs;
        }
        
        #elif Server
        public ClientInputState GetClientInputState(uint tick)
        {
            ClientInputState clientInputState = new ClientInputState();
            clientInputState = _clientInputStates[tick % InputBufferSize];
            
            // If the current input is null, but the last input isn't null, we just repeat the last input and save it as the current input
            if (clientInputState == null && _clientInputStates[(tick - 1) % InputBufferSize] != null)
            {
                clientInputState = _clientInputStates[(tick - 1) % InputBufferSize];
                _clientInputStates[tick % InputBufferSize] = clientInputState;
            }
            // If the current input is null, then we just create a new input with the current tick
            else if (clientInputState == null)
            {
                clientInputState = new ClientInputState();
                clientInputState.SetUp(tick, Vector2.zero, 0, 0);
                _clientInputStates[tick % InputBufferSize] = clientInputState;
            }

            return clientInputState;
        }
        #endif

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        private void OnClientInputsRPC(ClientInputState[] inputs)
        {
            #if Server
            uint newestInputTick = 0;
            
            foreach (ClientInputState input in inputs)
            {
                if (input == null) continue;
                _clientInputStates[input.Tick % InputBufferSize] = input;

                if (newestInputTick < input.Tick)
                    newestInputTick = input.Tick;
            }
            
            _bufferSize = (int) (newestInputTick - SimulationManager.CurrentTick);
            
            #endif
        }
    }
}