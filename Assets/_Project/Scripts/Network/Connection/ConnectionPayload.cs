using System.IO;

namespace LindoNoxStudio.Network.Connection
{
    /// <summary>
    /// Encodes and Decodes between Uuid | Username and byte array
    /// </summary>
    public static class ConnectionPayload
    {
        /// <summary>
        /// Encodes uuid and displayName to byte array
        /// </summary>
        /// <param name="uuid">Uuid | SteamId</param>
        /// <param name="displayName">DisplayName | SteamUserName</param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Decodes byte array payload to uuid and displayName
        /// </summary>
        /// <param name="payload">Connection payload</param>
        /// <returns></returns>
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