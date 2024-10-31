using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public struct PlayerState : IState
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        
        // Defining, that this is a Player State
        public StateType GetStateType() => StateType.Player;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref AngularVelocity);
        }
    }
}