using System;
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
        // Count of Ticks we save
        private const int InputBufferSize = 128;
        
        #if Client
        // My Client Inputs
        private ClientInputState[] _localClientInputStates = new ClientInputState[InputBufferSize];
        // The Tick of the first input we saved
        private uint _tickWeFirstStartedSavingInputs;
        
        // References
        private PlayerInput _playerInput;
        
        #elif Server
        // Client Inputs
        private ClientInputState[] _clientInputStates = new ClientInputState[InputBufferSize];
        #endif
        
        // Calculated Buffer Size
        [HideInInspector] public int _bufferSize = 3;
        
        public override void OnNetworkSpawn()
        {
            #if Client
            // Referencing
            _playerInput = GetComponent<PlayerInput>();
            #endif
        }

        #if Client
        
        /// <summary>
        /// Get's the current Input and save's it to list
        /// </summary>
        /// <param name="tick"></param>
        public void SaveInput(uint tick)
        {
            // Getting the current input state
            ClientInputState inputForThisTick = GetClientInputState(tick);
            
            // Saving the input in the dictionary
            _localClientInputStates[tick % InputBufferSize] = inputForThisTick;

            // Setting the variable for later use
            if (_tickWeFirstStartedSavingInputs == 0)
                _tickWeFirstStartedSavingInputs = tick;
        }
        
        /// <summary>
        /// Sends the last inputs to the Server
        /// </summary>
        public void SendInputs()
        {
            // If we haven't saved any inputs yet, then we don't have anything to send
            if (_tickWeFirstStartedSavingInputs == 0) return;
            
            // Sending the input with some last inputs to the server
            // Todo: Dynamic input array size (currently it's static on 15)
            OnClientInputsRPC(GetInputsToSend(15));
        }

        /// <summary>
        /// Returns an old ClientInputState or the CurrentCLientInputState. Depending on if we already have the input or not
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        public ClientInputState GetClientInputState(uint tick)
        {
            // Check if we already have this ClientInputState. If we have, we return this.
            ClientInputState clientInputState = _localClientInputStates[tick % InputBufferSize];
            if (clientInputState != null)
            {
                if (clientInputState.Tick == tick)
                {
                    return clientInputState;
                }
            }
            else
            {
                clientInputState = new ClientInputState();                
            }
            
            // We don't have the ClientInputState, so we return the current Input State
            
            // Getting Inputs
            Vector2 cycle;
            float pedals;
            float throttle;
            cycle = _playerInput.actions["Cycle"].ReadValue<Vector2>();
            pedals = _playerInput.actions["Pedals"].ReadValue<float>();
            throttle = _playerInput.actions["Throttle"].ReadValue<float>();

            // Setting rotation
            try
            {
                PlayerController playerController = NetworkPlayer.LocalNetworkPlayer._playerController;
                clientInputState.SetUp(tick, cycle, pedals, throttle);
            }
            catch (NullReferenceException e)
            {
                clientInputState.SetUp(tick, cycle, pedals, throttle);
            }

            return clientInputState;
        }
        
        /// <summary>
        /// Returns an array of ClientInputStates of the localClientInputStates. The array has the size of requestedSize. But If we don't have enough inputs, we return less inputs
        /// </summary>
        /// <param name="requestedSize">The ideal size of the array</param>
        /// <returns></returns>
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
        /// <summary>
        /// Returns a input, the Server can use.
        /// If we have the input that we want, we return the Input.
        /// If we don't have Client Inputs, we return a no input Input
        /// If we don't have the current Input, we return the last Input, that we used
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
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
                clientInputState.SetUp(tick, Vector2.zero, 0);
                _clientInputStates[tick % InputBufferSize] = clientInputState;
            }

            return clientInputState;
        }
        #endif
        
        /// <summary>
        /// Remote Procedural Call: The Client Sends his inputs to the Server
        /// </summary>
        /// <param name="inputs"></param>
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