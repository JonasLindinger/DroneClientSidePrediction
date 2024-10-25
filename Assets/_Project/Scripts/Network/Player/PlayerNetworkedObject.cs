using LindoNoxStudio.Network.Simulation;
using UnityEngine;

namespace LindoNoxStudio.Network.Player
{
    public class PlayerNetworkedObject : NetworkedObject
    {
        #if Client
        private PlayerState[] _snapshots = new PlayerState[StateBufferSize];
        
        private PlayerController _playerController;

        public override void OnNetworkSpawn()
        {
            _playerController = GetComponent<PlayerController>();
        }

        public override void TakeSnapshot(uint tick)
        {
            // Take a snapshot of the player's state at the given tick
            PlayerState currentState = _playerController.GetState(tick);
            _snapshots[tick % StateBufferSize] = currentState;
        }
        
        public override void ApplySnapshot(uint tick)
        {
            // Apply the snapshot of the player's state at the given tick
            PlayerState snapshot = _snapshots[tick % StateBufferSize];

            // Check if we have the right Snapshot
            if (snapshot == null)
            {
                Debug.Log("Something went wrong.");
                return;
            }
            else if (snapshot.Tick != tick)
            {
                Debug.Log("Something went wrong.");
                return;
            }
            
            // Apply the snapshot
            _playerController.ApplyState(snapshot);
            Debug.Log("Rollback to tick: " + tick + " CurrentTick: " + SimulationManager.CurrentTick);
        }
        
        public void ApplySnapshot(PlayerState snapshot)
        {
            // Check if we have the right Snapshot
            if (snapshot == null)
            {
                Debug.Log("Something went wrong.");
                return;
            }
            
            // Apply the snapshot
            _playerController.ApplyState(snapshot);
        }

        public void OverrideRotation(uint tick, Vector3 rotation)
        {
            // Apply the snapshot of the player's state at the given tick
            PlayerState snapshot = _snapshots[tick % StateBufferSize];

            // Check if we have the right Snapshot
            if (snapshot == null)
            {
                Debug.Log("Something went wrong.");
                return;
            }
            else if (snapshot.Tick != tick)
            {
                Debug.Log("Something went wrong.");
                return;
            }
            
            snapshot.Rotation = rotation;
            Debug.Log("Worked: " + (snapshot.Rotation == rotation ? "yes" : "no"));
        }
        
        public PlayerState GetSnapshot(uint tick)
        {
            return _snapshots[tick % StateBufferSize];
        }
        #endif
    }
}