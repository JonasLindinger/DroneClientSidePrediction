using LindoNoxStudio.Network.Ball;
using LindoNoxStudio.Network.Simulation;
using UnityEngine;
using IState = LindoNoxStudio.Network.Simulation.IState;

namespace LindoNoxStudio.Network.Ball
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallTracker : NetworkedObject
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
            return new BallState()
            {
                Position = transform.position,
                Velocity = _rb.linearVelocity,
                AngularVelocity = _rb.angularVelocity
            };
        }

        public override void ApplyState(IState state)
        {
            // Return early if state is not BallState
            if (!(state is BallState ballState))
                return;
            
            transform.position = ballState.Position;
            _rb.angularVelocity = ballState.AngularVelocity;
            _rb.linearVelocity = ballState.Velocity;
        }
    }
}