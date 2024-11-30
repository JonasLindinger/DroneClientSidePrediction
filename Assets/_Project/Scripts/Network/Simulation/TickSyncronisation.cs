using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace LindoNoxStudio.Network.Simulation
{
    public class TickSyncronisation : NetworkBehaviour
    {
        // Make this a setting in the game settings
        public WantedBufferSize _wantedBufferSize = WantedBufferSize.Balanced;
        
        #if Client
        // When the Client connects to the Server and we get the Server Tick, we add this ammount ontop of the tick, to get ahead of the Server
        public const int StartingTickOffset = 5;
        #endif
        
        #if Server
        public override void OnNetworkSpawn()
        {
            // We send the Current Tick to the Client, so that the Client can start the Tick System
            OnServerTickRPC(SimulationManager.CurrentTick);
        }
        
        /// <summary>
        /// We send the Buffer Size to the Client as a short (int => short)
        /// </summary>
        /// <param name="bufferSize"></param>
        public void SendBufferSize(int bufferSize)
        {
            short shortBufferSize = short.Parse(bufferSize.ToString());
            OnBufferSizeRPC(shortBufferSize);
        }
        
        #endif

        /// <summary>
        /// Remote procedural call.
        /// Server sends the Server Tick to the CLient, when the Client just connected
        /// </summary>
        /// <param name="serverTick"></param>
        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        private void OnServerTickRPC(uint serverTick)
        {
            #if Client
            SimulationManager.StartTickSystem(serverTick + StartingTickOffset);
            #endif
        }

        /// <summary>
        /// Remote procedural call.
        /// The Server sends the Clients Buffer Size to the Client and he checks if he want's to adjust.
        /// And when he should adjust, he clamps the value and sends it to the SimulationManager
        /// </summary>
        /// <param name="bufferSize">Server Buffer Size</param>
        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        private void OnBufferSizeRPC(short bufferSize)
        {
            #if Client
            
            int wantedBufferSize = (int) _wantedBufferSize;

            short tickAdjustment = short.Parse((wantedBufferSize - bufferSize).ToString());

            // If there is nothing to adjust, return.
            if (tickAdjustment == 0) 
                return;
            
            // Limiting the tick adjustment to +/-10 ticks.
            if (tickAdjustment > 10)
                tickAdjustment = 10;
            else if (tickAdjustment < -10)
                tickAdjustment = -10;

            // Skipping 1, 2 or 3 ticks isn't worth it.
            if (tickAdjustment < 0 && tickAdjustment >= 3)
                return;
            
            Debug.LogWarning("Adjusting tick");
            SimulationManager.AdjustTick(tickAdjustment);

            #endif
        }
    }
}