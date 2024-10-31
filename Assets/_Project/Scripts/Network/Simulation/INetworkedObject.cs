namespace LindoNoxStudio.Network.Simulation
{
    public interface INetworkedObject
    {
        ulong NetworkObjectId { get; }
        
        void Register();
        IState GetCurrentState();
        void ApplyState(IState state);
    }
}