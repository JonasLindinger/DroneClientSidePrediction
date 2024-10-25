namespace LindoNoxStudio.Scenes
{
    public class SceneOperation
    {
        public int ID;
        
        public int SceneIndex;
        public int ActiveSceneIndex;
        /// <summary>
        /// Defines if this Operation is a scene loading or a scene unloading operation.
        /// </summary>
        public bool IsLoadingOperation;
    }
}