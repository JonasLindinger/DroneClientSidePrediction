using LindoNoxStudio.Network.Connection;
using LindoNoxStudio.Network.Player;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public class SimulationManager : MonoBehaviour
    {
        // Instance for Singleton reference
        public static SimulationManager Instance { get; private set; }
        
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
        
        private void Start()
        {
            // Setting instance
            if (Instance != null)
            {
                Debug.LogError("Duplicate found");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        private void OnDestroy()
        {
            // Setting Instance to null, if we are the Instance
            if (!Instance) return;
            if (Instance != this) return;
            
            Instance = null;
        }

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
            
            // Setup the Network Tick System and subscribing to it
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

        #if Client
        /// <summary>
        /// Runs every physics tick.
        /// Saves the local client input state.
        /// </summary>
        /// <param name="tick"></param>
        public static void SaveLocalClientInput(uint tick)
        {
            if (NetworkClient.LocalClient == null) return;
            if (NetworkClient.LocalClient._input == null) return;
            
            // Save and send inputs
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
            // Schedule
            // Physics
            // Input
            // GameState

            #if Client
            SaveLocalClientInput(tick);
            #endif

            RunPhysicsTick(tick);
        }

        public static void RunPhysicsTick(uint tick)
        {
            //
            // 1. Handle Physics
            //
            
            // Simulating physics for the time between ticks
            Physics.Simulate(PhysicsTickSystem.TimeBetweenTicks);
            
            //
            // 2. Handle Input
            //
            
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
            
            //
            // 3. Handle Game State
            //
            
            SnapshotManager.TakeSnapshot(tick);
        }

        public static void HandleNetworkTick(uint tick)
        {
            #if Client
            // Send Client input
            if (NetworkClient.LocalClient != null)
            {
                if (NetworkClient.LocalClient._input != null)
                {
                    // Save and send inputs
                    NetworkClient.LocalClient._input.SendInputs();
                }
            }
            #elif Server
            // Send Game State to all players
            foreach (Client client in Client.Clients)
            {
                if (!client.NetworkPlayer) continue;
                client.NetworkPlayer.OnServerGameStateRPC(SnapshotManager.GetLatestSnapshot(), client.NetworkClient._input.GetClientInputState(tick + 1));
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