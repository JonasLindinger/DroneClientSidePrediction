using System.IO;

namespace LindoNoxStudio.Network.Connection
{
    public static class ConnectionPayload
    {
        public static byte[] Encode(ulong uuid, string displayName)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memoryStream))
                {
                    // Write the ulong (uuid)
                    writer.Write(uuid);

                    // Write the string (displayName)
                    writer.Write(displayName);
                }

                // Return the byte array
                return memoryStream.ToArray();
            }
        }
        
        public static (ulong uuid, string displayName) Decode(byte[] payload)
        {
            using (MemoryStream memoryStream = new MemoryStream(payload))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    // Read the ulong (uuid)
                    ulong uuid = reader.ReadUInt64();

                    // Read the string (displayName)
                    string displayName = reader.ReadString();

                    // Return the tuple with decoded data
                    return (uuid, displayName);
                }
            }
        }
    }
}