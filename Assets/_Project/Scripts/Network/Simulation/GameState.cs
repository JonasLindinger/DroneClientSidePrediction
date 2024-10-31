using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LindoNoxStudio.Network.Ball;
using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Simulation
{
    public class GameState : INetworkSerializable
    {
        public uint Tick;
        public Dictionary<ulong, IState> States = new Dictionary<ulong, IState>(); // Id, data
        
        // Implement the NetworkSerialize method for both sending and receiving
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // Serializing Tick
            serializer.SerializeValue(ref Tick);
            
            // Serializing Dictionary

            #region Dictionary
            if (serializer.IsWriter) // If this is the sending side
            {
                int count = States.Count;
                serializer.SerializeValue(ref count); // Serialize the count

                foreach (var kvp in States)
                {
                    ulong networkId = kvp.Key;
                    IState state = kvp.Value;

                    // Serialize the networkId
                    serializer.SerializeValue(ref networkId);

                    // Get the state type and serialize it
                    StateType stateType = state.GetStateType();
                    serializer.SerializeValue(ref stateType);

                    // Serialize the state based on its type
                    switch (stateType)
                    {
                        case StateType.Player:
                            PlayerState playerState = (PlayerState)state;
                            serializer.SerializeValue(ref playerState);
                            break;

                        case StateType.Ball:
                            BallState ballState = (BallState)state;
                            serializer.SerializeValue(ref ballState);
                            break;

                        // Add cases for other state types as necessary
                    }
                }
            }
            else // If this is the receiving side
            {
                int count = 0;
                serializer.SerializeValue(ref count); // Read the count

                for (int i = 0; i < count; i++)
                {
                    ulong networkId = 0;
                    serializer.SerializeValue(ref networkId); // Read the networkId

                    StateType stateType = StateType.Player; // Default
                    serializer.SerializeValue(ref stateType); // Read the state type

                    IState state = null;

                    switch (stateType)
                    {
                        case StateType.Player:
                            PlayerState playerState = new PlayerState();
                            serializer.SerializeValue(ref playerState); // Deserialize PlayerState
                            state = playerState;
                            break;

                        case StateType.Ball:
                            BallState ballState = new BallState();
                            serializer.SerializeValue(ref ballState); // Deserialize BallState
                            state = ballState;
                            break;

                        // Add cases for other state types as necessary
                    }

                    if (state != null)
                    {
                        States[networkId] = state; // Add the state to the dictionary
                    }
                }
            }
            #endregion
        }
    }
}