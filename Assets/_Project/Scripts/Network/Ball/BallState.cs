using LindoNoxStudio.Network.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Ball
{
    public struct BallState : IState
    {
        public Vector3 Position;
        // Todo: Add Rotation

        // Defining, that this is a Ball State
        public StateType GetStateType() => StateType.Ball;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
        }
    }
}