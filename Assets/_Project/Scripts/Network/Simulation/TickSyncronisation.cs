using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public class TickSyncronisation : NetworkBehaviour
    {
        [SerializeField] private WantedBufferSize _wantedBufferSize = WantedBufferSize.Balanced;
        
        #if Client
        public const int StartingTickOffset = 5;
        #endif
        
        #if Server
        public override void OnNetworkSpawn()
        {
            OnServerTickRPC(SimulationManager.CurrentTick);
        }
        
        public void SendBufferSize(int bufferSize)
        {
            short shortBufferSize = short.Parse(bufferSize.ToString());
            OnBufferSizeRPC(shortBufferSize);
        }
        
        #endif

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        private void OnServerTickRPC(uint serverTick)
        {
            #if Client
            SimulationManager.StartTickSystem(serverTick + StartingTickOffset);
            #endif
        }

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