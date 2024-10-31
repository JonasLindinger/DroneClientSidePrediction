using System.Collections.Generic;
using LindoNoxStudio.Network.Input;
using LindoNoxStudio.Network.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Connection
{
    [RequireComponent(typeof(ClientInput))]
    public class NetworkClient : NetworkBehaviour
    {
        #if Client
        // LocalClient Singleton for reference
        public static NetworkClient LocalClient { get; private set; }
        #elif Server
        // Client list for reference
        private static List<NetworkClient> Clients = new List<NetworkClient>();
        
        // The Client of this Object
        private Client _clientInfo;
        #endif
        
        // References
        [HideInInspector] public TickSyncronisation _tickSyncronisation;
        [HideInInspector] public ClientInput _input;
        
        public override void OnNetworkSpawn()
        {
            #if Client
            // Setting Instance
            LocalClient = this;
            #elif Server
            // Adding this to list
            Clients.Add(this);
            
            // Referencing this on the clientInfo
            _clientInfo = Client.GetClientByClientId(OwnerClientId);
            _clientInfo.NetworkClient = this;
            
            #endif
            
            // Referencing
            _tickSyncronisation = GetComponent<TickSyncronisation>();
            _input = GetComponent<ClientInput>();
            
            #if Server
            _input.ClientInfo = _clientInfo;
            #endif
        }

        public override void OnNetworkDespawn()
        {
            #if Client
            // Setting Instance to null
            LocalClient = null;
            #elif Server
            // Removing this from client list
            Clients.Remove(this);
            #endif
        } 
    }
}