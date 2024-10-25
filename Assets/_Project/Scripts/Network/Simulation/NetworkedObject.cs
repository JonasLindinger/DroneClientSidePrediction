using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public class NetworkedObject : NetworkBehaviour
    {
        #if Client
        
        private static List<NetworkedObject> _networkedObjects = new List<NetworkedObject>();
        
        public const int StateBufferSize = 128; // 8 bit | 1 byte

        public override void OnNetworkSpawn()
        {
            _networkedObjects.Add(this);
        }
        
        public override void OnNetworkDespawn()
        {
            _networkedObjects.Remove(this);
        }

        public static void Rollback(uint tick)
        {
            foreach (var obj in _networkedObjects)
            {
                if (!obj.NetworkObject.IsOwner)
                {
                    obj.ApplySnapshot(tick);
                }
            }
        }

        public static void OverwriteStates(uint tick)
        {
            foreach (var obj in _networkedObjects)
            {
                if (!obj.NetworkObject.IsOwner)
                {
                    obj.TakeSnapshot(tick);
                }
            }
        } 
        
        public virtual void TakeSnapshot(uint tick)
        {
            Debug.LogWarning("This method should be overridden in a derived class.");
        }

        public virtual void ApplySnapshot(uint tick)
        {
            Debug.LogWarning("This method should be overridden in a derived class.");
        }
        #endif
    }
}