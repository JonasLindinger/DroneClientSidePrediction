using UnityEngine;
using Unity.Netcode;

namespace LindoNoxStudio.Network.Input
{
    public class ClientInputState : INetworkSerializable
    {
        public uint Tick;
        public float Rotation;
        private byte _essentialKeys;
        
        // Decode Throttle
        public float Throttle => (_essentialKeys & (1 << 4)) != 0 ? 1 :
            (_essentialKeys & (1 << 5)) != 0 ? -1 : 0;
        
        // Decode Pedals
        public float Pedals => (_essentialKeys & (1 << 6)) != 0 ? 1 :
            (_essentialKeys & (1 << 7)) != 0 ? -1 : 0;
        
        #if Client
        public void SetUp(uint tick, float rotation, Vector2 moveInput, float throttle, float pedals)
        {
            Tick = tick;
            Rotation = rotation;
            
            // Reset the essentialKeys to 0 before setting new values
            _essentialKeys = 0;

            // Encode moveInput
            if (moveInput.y > 0) _essentialKeys |= 1 << 0; // Forward (y > 0)
            if (moveInput.x < 0) _essentialKeys |= 1 << 1; // Left (x < 0)
            if (moveInput.y < 0) _essentialKeys |= 1 << 2; // Backward (y < 0)
            if (moveInput.x > 0) _essentialKeys |= 1 << 3; // Right (x > 0)
    
            // Encode throttle (positive or negative or 0)
            if (throttle > 0) _essentialKeys |= 1 << 4; // Throttle forward
            if (throttle < 0) _essentialKeys |= 1 << 5; // Throttle backward
            
            // Encode pedals (positive or negative or 0)
            if (pedals > 0) _essentialKeys |= 1 << 6; // Pedals forward
            if (pedals < 0) _essentialKeys |= 1 << 7; // Pedals backward
        }
        #elif Server
        public void SetUp(uint tick, float rotation, Vector2 moveInput, float throttle, float pedals)
        {
            Tick = tick;
            Rotation = rotation;

            // Reset the essentialKeys to 0 before setting new values
            _essentialKeys = 0;

            // Encode moveInput
            if (moveInput.y > 0) _essentialKeys |= 1 << 0; // Forward (y > 0)
            if (moveInput.x < 0) _essentialKeys |= 1 << 1; // Left (x < 0)
            if (moveInput.y < 0) _essentialKeys |= 1 << 2; // Backward (y < 0)
            if (moveInput.x > 0) _essentialKeys |= 1 << 3; // Right (x > 0)
    
            // Encode throttle (positive or negative or 0)
            if (throttle > 0) _essentialKeys |= 1 << 4; // Throttle forward
            if (throttle < 0) _essentialKeys |= 1 << 5; // Throttle backward
            
            // Encode pedals (positive or negative or 0)
            if (pedals > 0) _essentialKeys |= 1 << 6; // Pedals forward
            if (pedals < 0) _essentialKeys |= 1 << 7; // Pedals backward
        }
        #endif

        // Decode moveInput
        public Vector2 GetCycle()
        {
            return new Vector2(
                (_essentialKeys & (1 << 1)) != 0 ? -1 :    // Left
                (_essentialKeys & (1 << 3)) != 0 ? 1 : 0,  // Right
                (_essentialKeys & (1 << 0)) != 0 ? 1 :     // Forward
                (_essentialKeys & (1 << 2)) != 0 ? -1 : 0  // Backward
            );
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref _essentialKeys);
        }
    }
}