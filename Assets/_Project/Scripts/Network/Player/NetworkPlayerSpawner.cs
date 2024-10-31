using System.Collections.Generic;
using UnityEngine;

namespace LindoNoxStudio.Network.Player
{
    public class NetworkPlayerSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private NetworkPlayer _playerPrefab;
        [Space(10)]
        [SerializeField] private List<Transform> _spawnPoints;
        
        #if Server
        // Instance for Singleton reference
        public static NetworkPlayerSpawner Instance { get; private set; }
        
        private void Start()
        {
            // Singleton referencing
            if (Instance != null)
            {
                Debug.LogError("Duplicate found");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        private void OnDestroy()
        {
            // Removing Singleton reference
            if (!Instance) return;
            if (Instance != this) return;
            
            Instance = null;
        }
        
        /// <summary>
        /// Instantiates a Network Player Prefab for a client with his ownership
        /// </summary>
        /// <param name="clientId">The Client Id of the Player</param>
        public void Spawn(ulong clientId)
        {
            // Choosing spawnpoint
            Transform spawnPoint = _spawnPoints[(int) (clientId - 1)];
            
            // Instantiate the player object on the server
            NetworkPlayer player = Instantiate(_playerPrefab, spawnPoint.position, spawnPoint.rotation);

            // Spawn the object on every client
            player.NetworkObject.SpawnWithOwnership(clientId);
        }
        #endif
    }
}