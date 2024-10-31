using LindoNoxStudio.Network.Simulation;
using UnityEngine;

namespace LindoNoxStudio.Network.Player
{
    [RequireComponent(typeof(Rigidbody))]
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
                Rotation = transform.eulerAngles,
                Velocity = _rb.linearVelocity,
                AngularVelocity = _rb.angularVelocity,
            };
        }

        public override void ApplyState(IState state)
        {
            // Return early if state is not PlayerState
            if (!(state is PlayerState playerState))
                return;

            transform.position = playerState.Position;
            transform.eulerAngles = playerState.Rotation;
            _rb.linearVelocity = playerState.Velocity;
            _rb.angularVelocity = playerState.AngularVelocity;
        }
    }
}