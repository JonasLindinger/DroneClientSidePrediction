using LindoNoxStudio.Network.Input;
using LindoNoxStudio.Network.Simulation;
using Unity.Netcode;
using UnityEngine;
using Client = LindoNoxStudio.Network.Connection.Client;
using NetworkClient = LindoNoxStudio.Network.Connection.NetworkClient;

namespace LindoNoxStudio.Network.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class NetworkPlayer : NetworkBehaviour
    {
        #if Client
        // Local Network Player Singleton reference
        public static NetworkPlayer LocalNetworkPlayer { get; private set; }
        
        #elif Server
        // Client info reference
        private Client _networkClient;
        #endif
        
        // References
        [HideInInspector] public PlayerController _playerController;
        [HideInInspector] public PlayerNetworkedObject _playerNetworkedObject;
        [HideInInspector] public PlayerStateSyncronisation _playerStateSyncronisation;
        
        public override void OnNetworkSpawn()
        {
            #if Client
            // Referencing Singleton
            if (IsOwner)
                LocalNetworkPlayer = this;
            #elif Server
            // Client Info referencing
            _networkClient = Client.GetClientByClientId(OwnerClientId);
            _networkClient.NetworkPlayer = this;
            #endif

            // Referencing
            _playerStateSyncronisation = GetComponent<PlayerStateSyncronisation>();
            _playerController = GetComponent<PlayerController>();
            _playerNetworkedObject = GetComponent<PlayerNetworkedObject>();
        }
        
        public override void OnNetworkDespawn()
        {
            #if Client
            // Removing Singleton reference
            if (IsOwner)
                LocalNetworkPlayer = null;
            #endif
        }

        #if Client
        /// <summary>
        /// Predicts and saves the local Game State
        /// </summary>
        /// <param name="tick"></param>
        public void PredictLocalState(uint tick)
        {
            // Getting input to process
            ClientInputState input = NetworkClient.LocalClient._input.GetClientInputState(tick);
            
            _playerNetworkedObject.TakeSnapshot(tick);

            // Process new input
            _playerController.OnInput(input);
        }
        #elif Server
        /// <summary>
        /// Sets and saves the Game State of this Player
        /// </summary>
        /// <param name="tick"></param>
        public void HandleState(uint tick)
        {   
            // Getting input to process
            ClientInputState input = _networkClient.NetworkClient._input.GetClientInputState(tick);

            _playerStateSyncronisation.SaveState(tick, input);
            
            // Process new input
            _playerController.OnInput(input);
        }
        #endif
    }
}