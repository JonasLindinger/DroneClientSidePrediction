using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Player
{
    public class PlayerState : INetworkSerializable
    {
        public uint Tick;
        public Vector3 Position;
        public Vector3 Velocity;

        public void SetUp(uint tick, Vector3 position, Vector3 velocity)
        {
            Tick = tick;
            Position = position;
            Velocity = velocity;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Velocity);
        }
    }
}