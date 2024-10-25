using LindoNoxStudio.Scenes;
using UnityEngine;

namespace LindoNoxStudio.Initialization
{
    public class Initializer : MonoBehaviour
    {
        private void Start()
        {
            #if Client
            
            SceneManager.Instance.LoadScene((int)SceneIndexes.NetworkLayer, (int)SceneIndexes.NetworkLayer, false);
            SceneManager.Instance.LoadScene((int)SceneIndexes.Game, (int)SceneIndexes.NetworkLayer);
            
            #elif Server
            
            SceneManager.Instance.LoadScene((int)SceneIndexes.NetworkLayer, (int)SceneIndexes.NetworkLayer, false);
            SceneManager.Instance.LoadScene((int)SceneIndexes.Game, (int)SceneIndexes.NetworkLayer);
            
            #endif
        }
    }
}