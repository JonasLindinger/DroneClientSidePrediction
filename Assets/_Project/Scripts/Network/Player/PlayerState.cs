using LindoNoxStudio.Network.Input;
using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Player
{
    public class PlayerState : INetworkSerializable
    {
        public uint Tick;
        /// <summary>
        /// x = Pitch, y = Roll, z = yaw
        /// </summary>
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Velocity;

        public void SetUp(uint tick, Vector3 position, Vector3 rotation, Vector3 velocity)
        {
            Tick = tick;
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref Velocity);
        }
    }
}