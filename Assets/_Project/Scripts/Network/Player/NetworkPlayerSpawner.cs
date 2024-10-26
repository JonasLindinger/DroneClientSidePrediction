using System.Collections.Generic;
using LindoNoxStudio.Network.Connection;
using UnityEngine;
using UnityEngine.Serialization;

namespace LindoNoxStudio.Network.Player
{
    public class NetworkPlayerSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private NetworkPlayer _playerPrefab;
        [Space(10)]
        [SerializeField] private List<Transform> _spawnPoints;
        
        #if Server
        public static NetworkPlayerSpawner Instance { get; private set; }
        
        private void Start()
        {
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
            if (!Instance) return;
            if (Instance != this) return;
            
            Instance = null;
        }
        
        public void Spawn(ulong clientId)
        {
            Transform spawnPoint = _spawnPoints[(int) (clientId - 1)];
            
            // Instantiate the player object on the server
            NetworkPlayer player = Instantiate(_playerPrefab, spawnPoint.position, spawnPoint.rotation);

            // Spawn the object on every client
            player.NetworkObject.SpawnWithOwnership(clientId);
        }
        #endif
    }
}