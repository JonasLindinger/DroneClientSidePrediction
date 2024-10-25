using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LindoNoxStudio.Network.Input;
using LindoNoxStudio.Network.Simulation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace LindoNoxStudio.Network.Connection
{
    [RequireComponent(typeof(ClientInput))]
    public class NetworkClient : NetworkBehaviour
    {
        #if Client
        public static NetworkClient LocalClient { get; private set; }
        
        #elif Server
        public static List<NetworkClient> Clients = new List<NetworkClient>();
        private Client _networkClient;
        #endif
        
        [HideInInspector] public TickSyncronisation _tickSyncronisation;
        [HideInInspector] public ClientInput _input;
        
        public override void OnNetworkSpawn()
        {
            #if Client
            
            LocalClient = this;
            Debug.Log("Local client spawned");
            
            #elif Server
            
            Clients.Add(this);
            Debug.Log("Client spawned");
            
            _networkClient = Client.GetClientByClientId(OwnerClientId);
            _networkClient.NetworkClient = this;
            
            #endif
            
            _tickSyncronisation = GetComponent<TickSyncronisation>();
            _input = GetComponent<ClientInput>();
        }

        public override void OnNetworkDespawn()
        {
            #if Client
            
            LocalClient = null;
            Debug.Log("Local despawned");
            
            #elif Server
            
            Clients.Remove(this);
            Debug.Log("Client despawned");
            
            #endif
        } 
    }
}