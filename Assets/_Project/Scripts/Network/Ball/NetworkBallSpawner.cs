using UnityEngine;

namespace LindoNoxStudio.Network.Ball
{
    public class NetworkBallSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NetworkBall _networkBall;
        
        #if Server
        // Instance for Singleton reference
        public static NetworkBallSpawner Instance { get; private set; }
        
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
        
        /// <summary>
        /// Instantiates a Network Ball Prefab
        /// </summary>
        public void Spawn()
        {
            // Instantiate the player object on the server
            NetworkBall client = Instantiate(_networkBall);
            
            client.NetworkObject.Spawn();
        }
        #endif
    }
}