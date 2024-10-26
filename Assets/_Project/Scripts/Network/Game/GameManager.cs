using System.Threading.Tasks;
using LindoNoxStudio.Network.Connection;
using LindoNoxStudio.Network.Player;
using UnityEngine;

namespace LindoNoxStudio.Network.Game
{
    public static class GameManager
    {
        public static GameStatus GameStatus { get; private set; } = GameStatus.WaitingForPlayers;

        #if Server
        public static async Task StartGame()
        {
            GameStatus = GameStatus.Starting;

            await Task.Delay(3000);
            
            GameStatus = GameStatus.Started;
            
            SpawnPlayers();
            
            Debug.Log("Game Started");
        }

        private static void SpawnPlayers()
        {
            Client[] clients = Client.Clients.ToArray();
            foreach (var client in clients)
            {
                NetworkPlayerSpawner.Instance.Spawn(client.ClientId);
            }
        }
        #endif
    }
}