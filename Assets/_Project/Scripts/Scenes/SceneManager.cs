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
        private int _currentActiveSceneIndex;
        
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
        
        public void AddSceneOperationToQueue(SceneOperationType operationType, int sceneIndex, int activeScene = -1)
        {
            SceneOperation operation = new SceneOperation
            {
                ID = Random.Range(0, 2100000000),
                SceneIndex = sceneIndex,
                ActiveSceneIndex = activeScene,
                OperationType = operationType
            };
            
            _sceneQueue.Enqueue(operation);
        }
        
        public void LoadScene(int sceneIndex, int activeScene = -1)
        {
            AddSceneOperationToQueue(SceneOperationType.Loading, sceneIndex, activeScene);
            
            RunSceneOperations();
        }
        
        public void UnLoadScene(int sceneIndex, int activeScene = -1)
        {
            AddSceneOperationToQueue(SceneOperationType.Unloading, sceneIndex, activeScene);
            
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
            AsyncOperation loadingOperation = new AsyncOperation();
            
            // Creating operation and starting it.
            switch (operationData.OperationType)
            {
                case SceneOperationType.Loading:
                    loadingOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                        operationData.SceneIndex,
                        LoadSceneMode.Additive
                    );
                    break;
                case SceneOperationType.Unloading:
                    loadingOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                        operationData.SceneIndex
                    );
                    break;
            }
            
            // Waiting until the operation is Done.
            while (!loadingOperation.isDone)
                await Task.Delay(1);
            
            // Setting the correct active scene.
            try
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(
                    UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(operationData.ActiveSceneIndex)
                );
            }
            catch (System.Exception e)
            {
                // We can't set the active scene, because the active scene isn't loaded!
                Debug.LogWarning(e);
            }
            
            // Setting flag and loading screen deactive.
            _isOperating = false;
            _loadingScreen.SetActive(false);
            
            // Repeat this method.
            RunSceneOperations();
        }
    }   
}
