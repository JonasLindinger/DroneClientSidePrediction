using LindoNoxStudio.Network.Connection;
using LindoNoxStudio.Network.Player;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public class SimulationManager : MonoBehaviour
    {
        // General settings. !Server and CLient have to have the same values!
        public const int PhysicsTickRate = 120;
        public const int NetworkTickRate = 60;
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
        public static TickSystem NetworkTickSystem { get; private set; }
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
            PhysicsTickSystem.OnTick += HandlePhysicsTick;
            
            // Setup the Physics Tick System and subscribing to it
            NetworkTickSystem = new TickSystem(NetworkTickRate, startingTick);
            NetworkTickSystem.OnTick += HandleNetworkTick;
            
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
            if (NetworkTickSystem != null)
                NetworkTickSystem.Update(Time.deltaTime);
            #if Server
            if (AdjustmentTickSystem != null)
                AdjustmentTickSystem.Update(Time.deltaTime);
            #endif
        }

        /// <summary>
        /// Runs every network tick.
        /// Client sends inputs, Server sends game states
        /// </summary>
        /// <param name="tick"></param>
        public static void HandleNetworkTick(uint tick)
        {
            #if Client
            if (NetworkClient.LocalClient == null) return;
            if (NetworkClient.LocalClient._input == null) return;
            NetworkClient.LocalClient._input.SendInputs();
            #elif Server
            // Send Game State to all players
            foreach (Client client in Client.Clients)
            {
                if (!client.NetworkPlayer) continue;
                client.NetworkPlayer.OnServerGameStateRPC(SnapshotManager.GetLatestGameState());
            }
            #endif
        }

        #if Client
        /// <summary>
        /// Client saves his inputs
        /// </summary>
        public static void SaveInput(uint tick)
        {
            // Save ande send inputs
            NetworkClient.LocalClient._input.SaveInput(tick);
        }
        #endif
        
        /// <summary>
        /// Runs every physics tick.
        /// Runs Physics and predicts the client state if we are the client and if we are the server he updates the players
        /// </summary>
        /// <param name="tick"></param>
        public static void HandlePhysicsTick(uint tick)
        {
            #if Client
            SaveInput(tick);
            #endif

            RunPhysicsTick(tick);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static void RunPhysicsTick(uint tick, bool isReconciliation = false, bool localPlayerPredictionWasWrong = false)
        {
            //
            // 1. Handle Physics
            //
            
            // Simulating physics for the time between ticks
            Physics.Simulate(PhysicsTickSystem.TimeBetweenTicks);
            
            #if Client
            //
            // 2. Handle Input
            //
                
            // Predicting local player state
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
            
            //
            // 3. Save Game State
            //
            
            SnapshotManager.TakeSnapshot(tick, localPlayerPredictionWasWrong);
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
        
        #endif
        
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