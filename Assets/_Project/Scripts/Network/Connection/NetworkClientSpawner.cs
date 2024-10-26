using UnityEngine;

namespace LindoNoxStudio.Network.Connection
{
    public class NetworkClientSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private NetworkClient _clientPrefab;
        
        #if Server
        // Instance for Singleton reference
        public static NetworkClientSpawner Instance { get; private set; }
        
        private void Start()
        {
            // Setting instance
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
            // Setting Instance to null, if we are the Instance
            if (!Instance) return;
            if (Instance != this) return;
            
            Instance = null;
        }
        
        public void Spawn(ulong clientId)
        {
            // Instantiate the player object on the server
            NetworkClient client = Instantiate(_clientPrefab);

            // Spawn the object on the specific client
            client.NetworkObject.DontDestroyWithOwner = true;
            client.NetworkObject.SpawnWithObservers = false;
            client.NetworkObject.SpawnWithOwnership(clientId);
            client.NetworkObject.NetworkShow(clientId);
        }
        #endif
    }
}