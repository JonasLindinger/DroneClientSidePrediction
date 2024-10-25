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
        public static NetworkPlayer LocalNetworkPlayer { get; private set; }
        
        #elif Server
        
        private Client _networkClient;
        #endif
        
        [HideInInspector] public PlayerController _playerController;
        private PlayerNetworkedObject _playerNetworkedObject;
        [HideInInspector] public PlayerStateSyncronisation _playerStateSyncronisation;
        
        public override void OnNetworkSpawn()
        {
            #if Client
            if (IsOwner)
                LocalNetworkPlayer = this;
            #elif Server
            _networkClient = Client.GetClientByClientId(OwnerClientId);
            _networkClient.NetworkPlayer = this;
            
            #endif

            _playerStateSyncronisation = GetComponent<PlayerStateSyncronisation>();
            _playerController = GetComponent<PlayerController>();
            _playerNetworkedObject = GetComponent<PlayerNetworkedObject>();
        }
        
        public override void OnNetworkDespawn()
        {
            #if Client
            if (IsOwner)
                LocalNetworkPlayer = null;
            #endif
        }

        #if Client
        public void PredictLocalState(uint tick)
        {
            // Getting input to process
            ClientInputState input = NetworkClient.LocalClient._input.GetClientInputState(tick);
            
            _playerNetworkedObject.TakeSnapshot(tick);

            // Process new input
            _playerController.OnInput(input);
        }
        #elif Server
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