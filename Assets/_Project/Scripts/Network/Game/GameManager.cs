using System.Threading.Tasks;
using LindoNoxStudio.Network.Ball;
using LindoNoxStudio.Network.Connection;
using LindoNoxStudio.Network.Player;
using UnityEngine;

namespace LindoNoxStudio.Network.Game
{
    public static class GameManager
    {
        // Game State. Default = Waiting for Players
        public static GameStatus GameStatus { get; private set; } = GameStatus.WaitingForPlayers;

        #if Server
        /// <summary>
        /// Starts the game.
        /// 1. Does countdown
        /// 2. Sets game flag
        /// 3. Spawns all player objects
        /// </summary>
        public static async Task StartGame()
        {
            // Changes the Game State to starting
            GameStatus = GameStatus.Starting;

            // Waiting 3 seconds
            await Task.Delay(3000);
            
            // Setting Game State to Started
            GameStatus = GameStatus.Started;
            
            // Spawning player Objects
            SpawnPlayers();
            NetworkBallSpawner.Instance.Spawn();
            
            // Logging
            Debug.Log("Game Started");
        }

        /// <summary>
        /// Spawns the player Object for every client
        /// </summary>
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