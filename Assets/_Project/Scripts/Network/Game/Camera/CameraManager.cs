using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace LindoNoxStudio.Network.Game.Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraManager : MonoBehaviour
    {
        #if Client
        // Instance for Singleton reference
        public static CameraManager Instance { get; private set; }
        
        // List of Camera Elements for camera tracking
        private List<CameraElement> _cameraElements = new List<CameraElement>();
        
        // Current Camera Element
        private CameraElement _currentCamera;
        
        // References
        private CinemachineCamera _camera;
        
        private void Start()
        {
            // Referencing
            _camera = GetComponent<CinemachineCamera>();
            
            // Setting Singleton
            if (Instance != null)
            {
                Debug.LogError("Duplicate found");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void OnDestroy()
        {
            // Removing Singleton Instance
            if (!Instance) return;
            if (Instance != this) return;
            
            Instance = null;
        }

        private void UpdateCamera(CameraElement currentCamera = null)
        {
            if (currentCamera == null) return;
            
            _currentCamera = currentCamera;
            
            _camera.LookAt = _currentCamera.transform;
            _camera.Follow = _currentCamera.transform;
        }

        /// <summary>
        /// Adds the cameraElement to list and updates the camera
        /// </summary>
        /// <param name="element"></param>
        /// <param name="makeThisMain">Should the new Camera Element be the active Camera Element</param>
        public void AddCameraElement(CameraElement element, bool makeThisMain)
        {
            _cameraElements.Add(element);

            if (makeThisMain)
                UpdateCamera(element);
        }

        /// <summary>
        /// Removes the cameraElement from list and updates the camera
        /// </summary>
        /// <param name="element"></param>
        public void RemoveCameraElement(CameraElement element)
        {
            _cameraElements.Remove(element);
            
            UpdateCamera();
        }
        #endif
    }
}