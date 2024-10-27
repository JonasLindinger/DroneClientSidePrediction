namespace LindoNoxStudio.Network.Simulation
{
    /// <summary>
    /// Presets of the Buffer Size we want.
    /// </summary>
    public enum WantedBufferSize : int
    {
        // Todo: Do a dynamic mode and calculate the best Buffer Size.
        LowLatency = 3, // This is for good internet
        Balanced = 6, // This is for average intenet
        HighLatency = 12 // This is for bad internet     
    }
}