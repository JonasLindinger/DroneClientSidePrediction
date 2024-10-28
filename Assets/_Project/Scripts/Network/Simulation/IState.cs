using Unity.Netcode;

namespace LindoNoxStudio.Network.Simulation
{
    public interface IState : INetworkSerializable
    {
        StateType GetStateType();
    }
}