using LindoNoxStudio.Scenes;
using UnityEngine;

namespace LindoNoxStudio.Initialization
{
    public class Initializer : MonoBehaviour
    {
        private void Start()
        {
            // Adding NetworkLayer to the Scene loading queue
            SceneManager.Instance.AddSceneOperationToQueue(SceneOperationType.Loading, (int)SceneIndexes.NetworkLayer, (int)SceneIndexes.NetworkLayer);
            // Adding Game to the Scene loading queue
            SceneManager.Instance.AddSceneOperationToQueue(SceneOperationType.Loading, (int)SceneIndexes.Game);
            
            // Loading scene operations
            SceneManager.Instance.RunSceneOperations();
        }
    }
}