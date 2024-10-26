namespace LindoNoxStudio.Scenes
{
    /// <summary>
    /// Loading Operation contains the information needed for the Operation
    /// </summary>
    public class SceneOperation
    {
        public int SceneIndex; // Scene we work with
        public int ActiveSceneIndex; // Scene that should be active after the operation
        public SceneOperationType OperationType; // Defines if it is a loading or unloading operation
    }
}