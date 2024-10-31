using System.Collections.Generic;
using LindoNoxStudio.Network.Player;
using UnityEngine;

namespace LindoNoxStudio.Network.Connection
{
    public class Client
    {
        // List of all Clients
        public static List<Client> Clients = new List<Client>();
        // List of Clients, that send input => are valid to receave game States
        public static List<Client> ClientThatSendInput = new List<Client>();
        
        // Unique name => Name(Uuid)
        public string UniqueName => DisplayName + "(" + Uuid + ")";
        
        // Unique player id to identify the player
        public ulong Uuid;
        // Network Client id to identify him from the network
        public ulong ClientId;
        // Client name to display to other players
        public string DisplayName;
        // Indicating flag to check if the player is online
        public bool IsOnline;
        
        // References to the Client object and the player object
        public NetworkClient NetworkClient;
        public NetworkPlayer NetworkPlayer;

        public static Client GetClientByUuid(ulong uuid)
        {
            return Clients.Find(c => c.Uuid == uuid);
        }
        
        public static Client GetClientByClientId(ulong clientId)
        {
            return Clients.Find(c => c.ClientId == clientId);
        }

        /// <summary>
        /// Adds this client to the input sender list.
        /// </summary>
        public void MarkAsInputSender()
        {
            if (ClientThatSendInput.Contains(this)) return;
            ClientThatSendInput.Add(this);
        }
        
        /// <summary>
        /// Adds the client to the list
        /// </summary>
        public void Add()
        {
            Clients.Add(this);
        }
        
        /// <summary>
        /// Removes the client from the list
        /// </summary>
        public void Remove()
        {
            Clients.Remove(this);
        }
        
        /// <summary>
        /// Marking the player as joined (online)
        /// </summary>
        public void Joined()
        {
            IsOnline = true;
        }

        /// <summary>
        /// Marking the player as left (offline)
        /// </summary>
        public void Left()
        {
            IsOnline = false;
        }
        
        /// <summary>
        /// Setting the new clientId and transferring ownership
        /// </summary>
        /// <param name="clientId"></param>
        public void Reconnected(ulong clientId)
        {
            // Changing the network client id
            ClientId = clientId;
            
            // Transfer ownership to the new client id
            NetworkClient.NetworkObject.ChangeOwnership(clientId);
            NetworkPlayer.NetworkObject.ChangeOwnership(clientId);
            
            Debug.Log("Reconnection worked: " + (GetClientByUuid(Uuid).ClientId == clientId));
        }
    }
}