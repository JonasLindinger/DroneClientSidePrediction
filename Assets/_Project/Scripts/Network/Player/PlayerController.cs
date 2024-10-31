using LindoNoxStudio.Network.Input;
using LindoNoxStudio.Network.Simulation;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace LindoNoxStudio.Network.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Settings")] 
        [SerializeField] private float _speed = 8f;
        [SerializeField] private float _sensitivity = 4f;
        [Space(10)]
        [Header("Animation")] 
        [SerializeField] private float _minMaxPitch = 30f;
        [SerializeField] private float _minMaxRoll = 30f;
        [SerializeField] private float _lerpSpeed = 2f;
        [Space(10)]
        [Header("References")]
        [SerializeField] private Transform _decoration;
        
        // Values
        #if Client
        public float _yaw;
        #elif Server
        private float _finalPitch;
        private float _finalRoll;
        #endif
        
        // References
        private Rigidbody _rb;

        public override void OnNetworkSpawn()
        {
            // Referencing
            _rb = GetComponent<Rigidbody>();
            
            // Rigidbody setup
            _rb.freezeRotation = true;
            _rb.useGravity = false;

            #if Client
            // Cursor
            if (IsLocalPlayer)
            {
                // Setting Cursor visibility and lockState
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                
                // Referencing
                CinemachineCamera vcam = Camera.main.GetComponent<CinemachineCamera>();
            }
            #endif
        }
        
        /// <summary>
        /// Moves and Rotates the player based on the input
        /// </summary>
        /// <param name="input">Input we use for the movement and rotation</param>
        public void OnInput(ClientInputState input)
        {
            if (input == null) return;

            // Calculate Rotation
            Quaternion rotation = Quaternion.Euler(0, input.Rotation, 0); // Calculating look direction
            
            // Apply Rotation
            _rb.MoveRotation(rotation);

            // Applying Force
            _rb.AddForce(GetEngineForce(input), ForceMode.Force);

            // Do Rotation (y on Client | x and z on Server)
            DoRotation(input);
        }
        
        /// <summary>
        /// Returns the engine Force
        /// </summary>
        /// <param name="input">Input we use for the engine force</param>
        /// <returns></returns>
        private Vector3 GetEngineForce(ClientInputState input)
        {
            Vector3 inputForce = new Vector3(input.GetCycle().x, input.Throttle, input.GetCycle().y).normalized;
            Vector3 engineForce = (transform.TransformDirection(inputForce) * _speed);

            return engineForce;
        }
        
        /// <summary>
        /// Rotates the player
        /// </summary>
        private void DoRotation(ClientInputState input)
        {
            #if Client
            // Modify value
            _yaw += input.Pedals * _sensitivity;
            
            // Calculate Rotation
            Quaternion rotation = Quaternion.Euler(0, _yaw, 0); // Calculating look direction
            
            // Apply Rotation
            _rb.MoveRotation(rotation);
            #elif Server
            return;
            // Modify values
            float pitch = input.GetCycle().y * _minMaxPitch;
            float roll = -input.GetCycle().x * _minMaxRoll;

            // Smoothing out values
            _finalPitch = Mathf.Lerp(_finalPitch, pitch, SimulationManager.PhysicsTickSystem.TimeBetweenTicks * _lerpSpeed);
            _finalRoll = Mathf.Lerp(_finalRoll, roll, SimulationManager.PhysicsTickSystem.TimeBetweenTicks * _lerpSpeed);
            
            // Calculate Rotation
            Vector3 rotation = new Vector3(_finalPitch, transform.rotation.y, _finalRoll); // Calculating drone rotation
            
            // Apply Rotation
            _decoration.eulerAngles = rotation;
            #endif
        }
    }
}