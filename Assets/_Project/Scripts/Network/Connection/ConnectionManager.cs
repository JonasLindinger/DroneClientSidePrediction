using System;
using LindoNoxStudio.Network.Game;
using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Connection
{
    public static class ConnectionManager
    {
        #if Server
        // Player count needed to start the game
        private const int WantedPlayerCount = 2;
        // Current player count
        private static int _currentPlayerCount;
        
        /// <summary>
        /// Handles Connection Approval. Decides if the player can join or not / If a client reconnects
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public static void OnClientJoinRequest(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            #region Payload Decoder

            // If the payload is null, we deny the request
            if (request.Payload == null)
            {
                response.Approved = false;
                response.Reason = "Payload is null";
                return;
            }
            
            // Trying to decode the payload. If we fail, we deny the request
            (ulong uuid, string displayName) payload;
            try
            {
                payload = ConnectionPayload.Decode(request.Payload);
            }
            catch (Exception e)
            {
                response.Approved = false;
                response.Reason = "Payload is not valid";
                return;
            }

            // If the DisplayName is null, we deny the request
            if (string.IsNullOrEmpty(payload.displayName))
            {
                response.Approved = false;
                response.Reason = "Payload is not valid";
                return;
            }
            // If the UUID is 0, we deny the request
            else if (payload.uuid == 0)
            {
                response.Approved = false;
                response.Reason = "Payload is not valid";
                return;
            }
            #endregion

            // Creating the client
            Client newClient = new Client()
            {
                Uuid = payload.uuid,
                ClientId = request.ClientNetworkId,
                DisplayName = payload.displayName,
            };
            
            // Checking if a client with the same uuid already exits
            if (Client.GetClientByUuid(newClient.Uuid) != null)
            {
                // Player already exists.
                Client existingClient = Client.GetClientByUuid(newClient.Uuid);
                
                // If the player is online, we deny the request
                if (existingClient.IsOnline)
                {
                    // Player tries to join, but he is already in the game and online
                    response.Reason = "You are already in the game and online!";
                    response.Approved = false;
                }
                // If the player is offline, we start the reconnection process
                else
                {
                    // Reconnect
                    response.Approved = true;

                    // Handle ownership and update clientId
                    existingClient.Reconnected(newClient.ClientId);
                    
                    Debug.Log(existingClient.UniqueName + "Client Reconnected");
                }
            }
            // There is no client with the same uuid
            else
            {
                // If we are waiting for players and the game hasn't started yet,
                // we accept the request, add the client and spawn the client object
                if (GameManager.GameStatus == GameStatus.WaitingForPlayers)
                {
                    // Game hasn't started yet
                    response.Approved = true;
                    
                    // Adding new client
                    newClient.Add();
                    NetworkClientSpawner.Instance.Spawn(newClient.ClientId);
                }
                // If the game isn't waiting for players, we deny the request
                else
                {
                    // Game already started
                    response.Reason = "Game already started";
                    response.Approved = false;
                }
            }
        }

        /// <summary>
        /// The Client was accepted. So we mark him as joined, increase the player count and check, if we can start the game
        /// </summary>
        /// <param name="newClient"></param>
        public static void OnClientJoined(Client newClient)
        {
            // Adding player
            newClient.Joined();
            _currentPlayerCount++;
            
            // Check for Game Start
            if (_currentPlayerCount == WantedPlayerCount)
                GameManager.StartGame();
            
            Debug.Log(newClient.UniqueName + " Joined");
        }
        
        /// <summary>
        /// A Client has left. So we check if we remove him from the server or mark him as disconnected.
        /// Game hasn't started => He disconnected, so we remove him and another player can join
        /// Game started        => He disconnected, but we leave a place for him to reconnect
        /// </summary>
        /// <param name="leftClient"></param>
        public static void OnClientLeft(Client leftClient)
        {
            // Waiting for players => remove him and hope for another player to join
            if (GameManager.GameStatus == GameStatus.WaitingForPlayers)
            {
                leftClient.Remove();
                _currentPlayerCount--;
                
                Debug.Log(leftClient.UniqueName + " Left");
            }
            // Game started => mark him as disconnected and hope that he reconnects
            else
            {
                leftClient.Left();

                Debug.Log(leftClient.UniqueName + " Disconnected");
            }
        }
        
        #endif 
    }
}