using LindoNoxStudio.Network.Ball;
using LindoNoxStudio.Network.Simulation;
using IState = LindoNoxStudio.Network.Simulation.IState;

namespace _Project.Scripts.Network.Ball
{
    public class BallTracker : NetworkedObject
    {
        public override IState GetCurrentState()
        {
            return new BallState()
            {
                Position = transform.position,
            };
        }

        public override void ApplyState(IState state)
        {
            // Return early if state is not BallState
            if (!(state is BallState ballState))
                return;
            
            transform.position = ballState.Position;
        }

        public override void ApplyNecessaryThings(IState state)
        {
            // Return early if state is not BallState
            if (!(state is BallState ballState))
                return;
            
            // Todo: Angular Velocity
        }
    }
}