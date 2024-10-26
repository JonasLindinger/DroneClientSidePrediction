using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LindoNoxStudio.Scenes
{
    public class SceneManager : MonoBehaviour
    {
        // Singleton reference
        public static SceneManager Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private GameObject _loadingScreen;

        // Queue of operations to load
        private Queue<SceneOperation> _operationQueue = new Queue<SceneOperation>();

        // Flag to indicate if we already run the queue
        private bool _isOperating;
        
        private void Start()
        {
            // Setting the Instance for Singleton reference
            if (Instance != null)
            {
                Debug.LogError("Duplicate found");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Don't destroy on Load for scene loading
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// Creates a Scene Operations and adds it to the operation queue
        /// </summary>
        /// <param name="operationType">Defines if it is a load or unload scene operation.</param>
        /// <param name="sceneIndex">The Scene we work with.</param>
        /// <param name="activeScene">The Scene we want to be active after the operation. | -1 = default for leaving it as it is.</param>
        public void AddSceneOperationToQueue(SceneOperationType operationType, int sceneIndex, int activeScene = -1)
        {
            SceneOperation operation = new SceneOperation
            {
                SceneIndex = sceneIndex,
                ActiveSceneIndex = activeScene,
                OperationType = operationType
            };
            
            _operationQueue.Enqueue(operation);
        }
        
        /// <summary>
        /// Creates a Load Scene Operation, Adds it to queue and runs the entire queue
        /// </summary>
        /// <param name="sceneIndex">The Scene we work with</param>
        /// <param name="activeScene">The Scene we want to be active after the operation. | -1 = default for leaving it as it is.</param>
        public void LoadScene(int sceneIndex, int activeScene = -1)
        {
            AddSceneOperationToQueue(SceneOperationType.Loading, sceneIndex, activeScene);
            
            RunSceneOperations();
        }
        
        /// <summary>
        /// Creates a Unload Scene Operation, Adds it to queue and runs the entire queue
        /// </summary>
        /// <param name="sceneIndex">The Scene we work with</param>
        /// <param name="activeScene">The Scene we want to be active after the operation. | -1 = default for leaving it as it is.</param>
        public void UnLoadScene(int sceneIndex, int activeScene = -1)
        {
            AddSceneOperationToQueue(SceneOperationType.Unloading, sceneIndex, activeScene);
            
            RunSceneOperations();
        }

        /// <summary>
        /// Runs the entire queue of scene operations
        /// </summary>
        public async Task RunSceneOperations()
        {
            // Checking if one of these chained methods is already running.
            // Checking if there is at least one operation to run.
            if (_isOperating || _operationQueue.Count == 0) return;

            // Setting flag and loading screen active.
            _isOperating = true;
            _loadingScreen.SetActive(true);

            SceneOperation operationData = _operationQueue.Dequeue();
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
            
            // Setting the correct active scene if we should change the active scene (not -1)
            if (operationData.ActiveSceneIndex != -1)
            {
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
            }
            
            // Setting flag and disabling the loading sceen.
            _isOperating = false;
            _loadingScreen.SetActive(false);
            
            // Repeat this method.
            RunSceneOperations();
        }
    }   
}
