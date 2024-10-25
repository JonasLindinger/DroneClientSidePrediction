using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LindoNoxStudio.Scenes
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private GameObject _loadingScreen;

        private Queue<SceneOperation> _sceneQueue = new Queue<SceneOperation>();

        private bool _isOperating;
        
        private void Start()
        {
            if (Instance != null)
            {
                Debug.LogError("Duplicate found");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void LoadScene(int sceneIndex, int activeScene, bool onlyAddToQueue = false)
        {
            SceneOperation operation = new SceneOperation
            {
                ID = Random.Range(0, 1000),
                SceneIndex = sceneIndex,
                ActiveSceneIndex = activeScene,
                IsLoadingOperation = true
            };
            
            _sceneQueue.Enqueue(operation);
            
            if (onlyAddToQueue) return;
            RunSceneOperations();
        }
        
        public void UnLoadScene(int sceneIndex, int activeScene, bool onlyAddToQueue = false)
        {
            SceneOperation operation = new SceneOperation
            {
                ID = Random.Range(0, 1000),
                SceneIndex = sceneIndex,
                ActiveSceneIndex = activeScene,
                IsLoadingOperation = false
            };
            
            _sceneQueue.Enqueue(operation);
            
            if (onlyAddToQueue) return;
            RunSceneOperations();
        }

        public async Task RunSceneOperations()
        {
            // Checking if one of these chained methods is already running.
            // Checking if there is at least one operation to run.
            if (_isOperating || _sceneQueue.Count == 0) return;

            // Setting flag and loading screen active.
            _isOperating = true;
            _loadingScreen.SetActive(true);

            SceneOperation operationData = _sceneQueue.Dequeue();
            AsyncOperation loadingOperation;
            
            // Creating operation and starting it.
            if (operationData.IsLoadingOperation)
            {
                loadingOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                    operationData.SceneIndex,
                    LoadSceneMode.Additive
                );
            }
            else
            {
                loadingOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                    operationData.SceneIndex
                );
            }
            
            // Waiting until the operation is Done.
            while (!loadingOperation.isDone)
                await Task.Delay(1);
            
            // Setting the correct active scene.
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(
                UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(operationData.ActiveSceneIndex)
                );
            
            // Setting flag and loading screen deactive.
            _isOperating = false;
            _loadingScreen.SetActive(false);
            
            // Repeat this method.
            RunSceneOperations();
        }
    }   
}
