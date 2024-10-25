using LindoNoxStudio.Network.Connection;
using LindoNoxStudio.Network.Player;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public static class SimulationManager
    {
        public const int PhysicsTickRate = 60;
        public const int StateTickRate = 60;
        #if Server
        public const int AdjustmentTickRate = 2;
        #endif
        
        public static uint CurrentTick => PhysicsTickSystem.CurrentTick;
        
        public static TickSystem PhysicsTickSystem { get; private set; }
        public static TickSystem StateTickSystem { get; private set; }
        #if Server
        public static TickSystem AdjustmentTickSystem { get; private set; }
        #endif

        public static void StartTickSystem(uint startingTick = 0)
        {
            // Freezing physics, so that we can run it manually
            Physics.simulationMode = SimulationMode.Script;
            
            // Startting tick systems
            PhysicsTickSystem = new TickSystem(PhysicsTickRate, startingTick);
            #if Client
            PhysicsTickSystem.OnTick += SaveInput;
            #endif
            PhysicsTickSystem.OnTick += HandlePhysicsTick;
            
            StateTickSystem = new TickSystem(StateTickRate);
            StateTickSystem.OnTick += HandleStateTick;
            
            #if Server
            AdjustmentTickSystem = new TickSystem(AdjustmentTickRate);
            AdjustmentTickSystem.OnTick += HandleAdjustmentTick;
            #endif
        }

        public static void Update(float deltaTime)
        {
            if (PhysicsTickSystem != null)
                PhysicsTickSystem.Update(deltaTime);
            if (StateTickSystem != null)
                StateTickSystem.Update(deltaTime);
            #if Server
            if (AdjustmentTickSystem != null)
                AdjustmentTickSystem.Update(deltaTime);
            #endif
        }

        public static void HandlePhysicsTick(uint tick)
        {
            // Simulating physics for the time between ticks
            Physics.Simulate(PhysicsTickSystem.TimeBetweenTicks);

            #if Client
            
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
        public static void AdjustTick(int ammount)
        {
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
        
        private static void SaveInput(uint tick)
        {
            // Saving input for the current tick
            if (!NetworkClient.LocalClient) return;
            if (!NetworkClient.LocalClient._input) return;
            NetworkClient.LocalClient._input.SaveInput(tick);
        }
        #endif
        
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