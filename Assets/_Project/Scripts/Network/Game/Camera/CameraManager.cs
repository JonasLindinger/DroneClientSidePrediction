using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace LindoNoxStudio.Network.Game.Camera
{
    #if Client
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraManager : MonoBehaviour
    {
        private List<CameraElement> _cameraElements = new List<CameraElement>();
        private CameraElement _currentCamera;
        private CinemachineCamera _camera;

        public static CameraManager Instance { get; private set; }
        
        private void Start()
        {
            _camera = GetComponent<CinemachineCamera>();
            
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
            if (!Instance) return;
            if (Instance != this) return;
            
            Instance = null;
        }

        private void UpdateCamera(CameraElement currentCamera = null)
        {
            if (currentCamera)
                _currentCamera = currentCamera;
            
            if (currentCamera == null) return;
            
            _camera.LookAt = _currentCamera.transform;
            _camera.Follow = _currentCamera.transform;
        }

        public void AddCameraElement(CameraElement element, bool makeThisMain)
        {
            _cameraElements.Add(element);

            if (makeThisMain)
                UpdateCamera(element);
            else
                UpdateCamera();
        }

        public void RemoveCameraElement(CameraElement element)
        {
            _cameraElements.Remove(element);
            
            UpdateCamera();
        }
    }
    #endif
}