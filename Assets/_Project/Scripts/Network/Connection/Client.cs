using System.Collections.Generic;
using LindoNoxStudio.Network.Player;
using UnityEngine;

namespace LindoNoxStudio.Network.Connection
{
    public class Client
    {
        public static List<Client> Clients = new List<Client>();
        
        public string UniqueName => DisplayName + "(" + Uuid + ")";
        
        public ulong Uuid;
        public ulong ClientId;
        
        public string DisplayName;
        public bool IsOnline;
        
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

        public void Add()
        {
            Clients.Add(this);
        }
        
        public void Remove()
        {
            Clients.Remove(this);
        }
        
        public void Joined()
        {
            IsOnline = true;
        }

        public void Left()
        {
            IsOnline = false;
        }
        
        public void Reconnected(ulong clientId)
        {
            ClientId = clientId;
            NetworkClient.NetworkObject.ChangeOwnership(clientId);
            NetworkPlayer.NetworkObject.ChangeOwnership(clientId);
            
            Debug.Log("Reconnectiong worked: " + (GetClientByUuid(Uuid).ClientId == clientId));
        }

        public void Reference(NetworkClient networkClient)
        {
            NetworkClient = networkClient;
        }

        public void Reference(NetworkPlayer networkPlayer)
        {
            NetworkPlayer = networkPlayer;
        }
    }
}