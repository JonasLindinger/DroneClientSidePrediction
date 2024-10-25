using System.Threading.Tasks;
using LindoNoxStudio.Network.Connection;
using LindoNoxStudio.Network.Player;
using UnityEngine;

namespace LindoNoxStudio.Network.Game
{
    public static class GameManager
    {
        public static GameState GameState { get; private set; } = GameState.WaitingForPlayers;

        #if Server
        public static async Task StartGame()
        {
            GameState = GameState.Starting;

            await Task.Delay(3000);
            
            GameState = GameState.Started;
            
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