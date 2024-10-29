using LindoNoxStudio.Network.Simulation;
using UnityEngine;

namespace LindoNoxStudio.Network.Player
{
    public class PlayerTracker : NetworkedObject
    {
        private Rigidbody _rb;

        protected override void Start()
        {
            // Calling the Start method of the base class
            base.Start();
            
            // Referencing
            _rb = GetComponent<Rigidbody>();
        }

        public override IState GetCurrentState()
        {
            return new PlayerState()
            {
                Position = transform.position,
                AngularVelocity = _rb.angularVelocity,
            };
        }

        public override void ApplyState(IState state)
        {
            // Return early if state is not PlayerState
            if (!(state is PlayerState playerState))
                return;

            transform.position = playerState.Position;
            _rb.angularVelocity = playerState.AngularVelocity;
        }

        public override void ApplyNecessaryThings(IState state)
        {
            // Return early if state is not PlayerState
            if (!(state is PlayerState playerState))
                return;
            
            // Apply velocity
            _rb.angularVelocity = playerState.AngularVelocity;
        }
    }
}