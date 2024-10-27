using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public class NetworkedObject : NetworkBehaviour
    {
        #if Client
        
        // Keeps track of all Networked Objects we have to reconcile.
        private static List<NetworkedObject> _networkedObjects = new List<NetworkedObject>();
        
        // Other scripts that use this class use this for there circular buffer arrays
        public const int StateBufferSize = 128; // 8 bit | 1 byte

        public override void OnNetworkSpawn()
        {
            // Let's add us to the list
            _networkedObjects.Add(this);
        }
        
        public override void OnNetworkDespawn()
        {
            // Let's remove us from the list
            _networkedObjects.Remove(this);
        }

        /// <summary>
        /// Rollback all networked objects to a specific tick
        /// </summary>
        /// <param name="tick"></param>
        public static void Rollback(uint tick)
        {
            // Todo: Check if every networked object has the correct state setup
            
            // Applying the snapshot of every single Networked Object
            foreach (var obj in _networkedObjects)
            {
                if (!obj.NetworkObject.IsOwner)
                {
                    obj.ApplySnapshot(tick);
                }
            }
        }

        /// <summary>
        /// Every Networked Object saves the current state for the tick
        /// </summary>
        /// <param name="tick"></param>
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
        
        /// <summary>
        /// The Networked Object takes a snapshot and saves it to the tick
        /// </summary>
        /// <param name="tick"></param>
        public virtual void TakeSnapshot(uint tick)
        {
            Debug.LogWarning("This method should be overridden in a derived class.");
        }

        /// <summary>
        /// The Networked Object applys the recoreded state for the tick.
        /// </summary>
        /// <param name="tick"></param>
        public virtual void ApplySnapshot(uint tick)
        {
            Debug.LogWarning("This method should be overridden in a derived class.");
        }
        #endif
    }
}