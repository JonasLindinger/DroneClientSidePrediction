using LindoNoxStudio.Network.Simulation;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LindoNoxStudio.Network.Connection
{
    public class NetworkStarter : MonoBehaviour
    {
        // Todo: Override the Connection data for real use
        // Connection Data to connect to
        private static ConnectionData _connectionData = new ConnectionData()
        {
            IP = "127.0.0.1", // Default ip = 127.0.0.1 (localHost) for testing purposes
            Port = 7778 // Default Port = 7778 for testing purposes
        };
        
        private void Start()
        {
            #if Client
            StartClient();
            #elif Server
            StartServer();
            #endif
        }

        private void Update()
        {
            SimulationManager.Update(Time.deltaTime);
        }

        /// <summary>
        /// Getting the Unity Transport of the NetworkManager and setting the ip and port to the connection data
        /// </summary>
        private void SetConnectionDataOnTransport()
        {
            // Getting the Unity Transport
            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            // Setting the connection data
            unityTransport.ConnectionData = new UnityTransport.ConnectionAddressData()
            {
                Address = _connectionData.IP,
                Port = _connectionData.Port
            };
        }
        
        #if Client
        /// <summary>
        /// Subscribes to events, creates connection payload for server and starting the client
        /// </summary>
        private void StartClient()
        {
            // Setting the connection data
            SetConnectionDataOnTransport();
            
            // Subscribe to Client Connect / Disconnect event
            NetworkManager.Singleton.OnConnectionEvent +=
                (NetworkManager networkManager, ConnectionEventData connectionEvent) =>
                {
                    // Only do something if we connect or disconnect
                    if (connectionEvent.ClientId != NetworkManager.Singleton.LocalClientId) return;
                    
                    switch (connectionEvent.EventType)
                    {
                        // On Connected to Server Event
                        case ConnectionEvent.ClientConnected:
                            Debug.Log("Client connected.");
                            break;
                        
                        // On Disconnected from Server Event
                        case ConnectionEvent.ClientDisconnected:
                            Debug.Log("Client disconnected. Reason: " + NetworkManager.Singleton.DisconnectReason);
                            break;
                    }
                };
            
            // Creating Connection Data, saving it
            // Todo: Set clientId to SteamId
            ulong clientId = (ulong) Random.Range(11111, 99999);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = ConnectionPayload.Encode(clientId, "Client " + clientId);
            
            // Starting client and logging the client status
            if (NetworkManager.Singleton.StartClient())
                Debug.Log("Client started.");
            else
                Debug.Log("Failed to start client.");
        }
        #elif Server
        private void StartServer()
        {
            // Setting the connection data
            SetConnectionDataOnTransport();

            // Subscribing to the connection approval event
            NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionManager.OnClientJoinRequest;
            // Subscribe to Client Connect / Disconnect event
            NetworkManager.Singleton.OnConnectionEvent +=
                (NetworkManager networkManager, ConnectionEventData connectionEvent) =>
                {
                    switch (connectionEvent.EventType)
                    {
                        case ConnectionEvent.ClientConnected:
                            // We approved the client, so we mark him as joined
                            ConnectionManager.OnClientJoined(Client.GetClientByClientId(connectionEvent.ClientId));
                            break;
                        case ConnectionEvent.ClientDisconnected:
                            // If we registered the client, we remove the client
                            Client leftClient = Client.GetClientByClientId(connectionEvent.ClientId);
                            if (leftClient == null) return;
                            ConnectionManager.OnClientLeft(leftClient);
                            break;
                    }
                };

            // Starting the Server, logging the server status and starting tick System
            if (NetworkManager.Singleton.StartServer())
            {
                SimulationManager.StartTickSystem();
                Debug.Log("Server started.");
            }
            else
                Debug.Log("Failed to start server.");
        } 
        #endif
    }
}