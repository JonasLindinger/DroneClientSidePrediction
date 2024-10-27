using LindoNoxStudio.Network.Simulation;
using UnityEngine;

namespace LindoNoxStudio.Network.Player
{
    public class PlayerNetworkedObject : NetworkedObject
    {
        #if Client
        // Array of previous player states
        // Todo: Rename Player STate to Snapshot ? And do one big Game State called Snapshot?
        private PlayerState[] _snapshots = new PlayerState[StateBufferSize];
        
        // References
        private PlayerController _playerController;

        public override void OnNetworkSpawn()
        {
            // Referencing
            _playerController = GetComponent<PlayerController>();
        }

        /// <summary>
        /// Takes the current player state and turns it into a Player State and saves it.
        /// </summary>
        /// <param name="tick"></param>
        public override void TakeSnapshot(uint tick)
        {
            // Take a snapshot of the player's state at the given tick
            PlayerState currentState = _playerController.GetState(tick);
            _snapshots[tick % StateBufferSize] = currentState;
        }
        
        /// <summary>
        /// Apply the Snapshot we saved for the given TIck
        /// </summary>
        /// <param name="tick"></param>
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
        
        /// <summary>
        /// Applys Snapshot we got.
        /// </summary>
        /// <param name="snapshot"></param>
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
        
        
        /// <summary>
        /// Returns the Snapshot for the given tick.
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        public PlayerState GetSnapshot(uint tick)
        {
            return _snapshots[tick % StateBufferSize];
        }
        #endif
    }
}