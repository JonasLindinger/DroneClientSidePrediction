using LindoNoxStudio.Network.Connection;
using LindoNoxStudio.Network.Player;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public class SimulationManager : MonoBehaviour
    {
        // General settings. !Server and CLient have to have the same values!
        public const int PhysicsTickRate = 60;
        #if Server
        // The ammount of time we send the tick adjustment rate to the Clients
        public const int AdjustmentTickRate = 1;
        #endif
        
        /// <summary>
        /// Returns the Current Physics Tick
        /// </summary>
        public static uint CurrentTick => PhysicsTickSystem.CurrentTick;
        
        // Tick System(s)
        public static TickSystem PhysicsTickSystem { get; private set; }
        #if Server
        public static TickSystem AdjustmentTickSystem { get; private set; }
        #endif

        /// <summary>
        /// Starts the tick system and sets the simulationMode to Script.
        /// </summary>
        /// <param name="startingTick">Optionaly you can set the starting physics tick</param>
        public static void StartTickSystem(uint startingTick = 0)
        {
            // Freezing physics, so that we can run it manually
            Physics.simulationMode = SimulationMode.Script;
            
            // Setup the Physics Tick System and subscribing to it
            PhysicsTickSystem = new TickSystem(PhysicsTickRate, startingTick);
            #if Client
            PhysicsTickSystem.OnTick += SaveInput;
            #endif
            PhysicsTickSystem.OnTick += HandlePhysicsTick;
            
            // Todo: Do this in the handle physics tick method and comment this out / delete this
            PhysicsTickSystem.OnTick += HandleStateTick;
            
            #if Server
            // Setup the Adjustment Tick System and subscribing to it
            AdjustmentTickSystem = new TickSystem(AdjustmentTickRate);
            AdjustmentTickSystem.OnTick += HandleAdjustmentTick;
            #endif
        }

        public void Update()
        {
            // Updating the Tick System(s)
            if (PhysicsTickSystem != null)
                PhysicsTickSystem.Update(Time.deltaTime);
            #if Server
            if (AdjustmentTickSystem != null)
                AdjustmentTickSystem.Update(Time.deltaTime);
            #endif
        }

        /// <summary>
        /// Runs every physics tick.
        /// Runs Physics and predicts the client state if we are the client and if we are the server he updates the players
        /// </summary>
        /// <param name="tick"></param>
        public static void HandlePhysicsTick(uint tick)
        {
            // Simulating physics for the time between ticks
            Physics.Simulate(PhysicsTickSystem.TimeBetweenTicks);

            #if Client
            
            // Predicting local player state and sending input to server
            if (NetworkPlayer.LocalNetworkPlayer)
                NetworkPlayer.LocalNetworkPlayer.PredictLocalState(tick);
            #elif Server
            // Update all players
            foreach (Client client in Client.Clients)
            {
                if (!client.NetworkPlayer) continue;
                client.NetworkPlayer.HandleState(tick);
            }
            #endif
        }
        
        // Todo: move this in the other method. And improve the reconciliation
        /// <summary>
        /// Does the same as physics tick, but we reconcile to the state.
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="isReaconciliation"></param>
        public static void HandlePhysicsTick(uint tick, bool isReaconciliation = false)
        {
            // Simulating physics for the time between ticks
            Physics.Simulate(PhysicsTickSystem.TimeBetweenTicks);

            #if Client
            // Rollback all other objects if isReaconciliation is true
            if (isReaconciliation)
                NetworkedObject.Rollback(tick);
            
            // Predicting local player state and sending input to server
            if (NetworkPlayer.LocalNetworkPlayer)
                NetworkPlayer.LocalNetworkPlayer.PredictLocalState(tick);
            #elif Server
            // Move all players
            foreach (Client client in Client.Clients)
            {
                if (!client.NetworkPlayer) continue;
                client.NetworkPlayer.HandleState(tick);
            }
            #endif
        }

        #if Client
        /// <summary>
        /// We adjust the physics tick system by either calculating more or calculating less ticks
        /// </summary>
        /// <param name="ammount"></param>
        public static void AdjustTick(int ammount)
        {
            // Todo: When the ticks are too hard apart (>10) then set the tick manually?
            if (ammount < 0)
            {
                ammount = Mathf.Abs(ammount);
                PhysicsTickSystem.SkipTick(ammount);
            }
            else
            {
                PhysicsTickSystem.CalculateExtraTicks(ammount);
            }
        }
        
        /// <summary>
        /// Saves the input of the local client
        /// </summary>
        /// <param name="tick"></param>
        private static void SaveInput(uint tick)
        {
            // Saving input for the current tick
            if (!NetworkClient.LocalClient) return;
            if (!NetworkClient.LocalClient._input) return;
            NetworkClient.LocalClient._input.SaveInput(tick);
        }
        #endif
        
        // Todo: Send Game States instead of Player States
        /// <summary>
        /// On the Client, we send the ClientInputStates and if we are Server, we send the Game State.
        /// </summary>
        /// <param name="tick"></param>
        private static void HandleStateTick(uint tick)
        {
            #if Client
            // Send inputs to server
            if (!NetworkClient.LocalClient) return;
            if (!NetworkClient.LocalClient._input) return;
            NetworkClient.LocalClient._input.SendInputs();
            #elif Server
            // Sending states to clients
            foreach (var client in Client.Clients)
            {
                if (!client.NetworkPlayer) continue;
                if (!client.NetworkPlayer._playerStateSyncronisation) continue;
                client.NetworkPlayer._playerStateSyncronisation.SendState();
            }
            #endif
        }
        
        #if Server
        /// <summary>
        /// We send the buffer Size to every client, so that they can adjust there tick system
        /// </summary>
        /// <param name="tick"></param>
        private static void HandleAdjustmentTick(uint tick)
        {
            // Sending bufferSize to clients
            foreach (var client in Client.Clients)
            {
                if (!client.NetworkClient) continue;
                if (!client.NetworkClient._tickSyncronisation) continue;
                client.NetworkClient._tickSyncronisation.SendBufferSize(client.NetworkClient._input._bufferSize);
            }
        }
        #endif
    }
}