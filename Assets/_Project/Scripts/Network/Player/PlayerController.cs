using LindoNoxStudio.Network.Input;
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
        [SerializeField] private float speed = 8f;
        [SerializeField] private float sensitivity = 4f;
        [Space(10)]
        [Header("Animation")] 
        [SerializeField] private float minMaxPitch = 30f;
        [SerializeField] private float minMaxRoll = 30f;
        [SerializeField] private float lerpSpeed = 2f;

        // Values
        [HideInInspector] public float finalPitch;
        [HideInInspector] public float finalRoll;
        [HideInInspector] public float yaw;
        
        // References
        private Rigidbody _rb;

        public override void OnNetworkSpawn()
        {
            // Referencing
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;

            #if Client
            // Cursor
            if (IsLocalPlayer)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                
                CinemachineCamera vcam = Camera.main.GetComponent<CinemachineCamera>();
            }
            #endif
        }
        
        public void OnInput(ClientInputState input)
        {
            if (input == null) return;

            // Set yaw for precise Predictions
            ApplyYaw(input.PlayerRotation);
            
            // Applying Force
            _rb.AddForce(GetEngineForce(input), ForceMode.Force);
            
            #if Client
            // Changing Yaw for next state
            DoYawRotation(input);
            #endif
            #if Server
            Rotate(input);
            #endif  
        }
        
        /// <summary>
        /// Returns the engine Force
        /// </summary>
        /// <returns></returns>
        private Vector3 GetEngineForce(ClientInputState input)
        {
            Vector3 inputForce = new Vector3(input.GetCycle().x, input.Throttle, input.GetCycle().y).normalized;
            Vector3 gravityCounterForce = Vector3.up * (_rb.mass * Physics.gravity.magnitude);
            Vector3 engineForce =
                gravityCounterForce + // Counter gravity
                (transform.TransformDirection(inputForce) * speed);  // Move Input * Power

            return engineForce;
        }

        private void ApplyYaw(float rotation)
        {
            yaw = rotation;
            Quaternion parentRotation = Quaternion.Euler(transform.localRotation.x, yaw, transform.localRotation.z);
            transform.localRotation = parentRotation;
        }

        #if Client
        private void DoYawRotation(ClientInputState input)
        {
            yaw += input.Pedals * sensitivity;
        }

        public void ApplyVisualRotation(float pitch, float roll)
        {
            // Calculate Rotation
            Quaternion visualRotation = Quaternion.Euler(finalPitch, 0, finalRoll); // Calculating drone rotation
            
            // Apply Rotation
            transform.GetChild(0).localRotation = visualRotation; // Rotating Drone
        }
        #elif Server
        private void Rotate(ClientInputState input)
        {
            // Modify values
            float pitch = input.GetCycle().y * minMaxPitch;
            float roll = -input.GetCycle().x * minMaxRoll;
                
            // Smoothing out values
            finalPitch = Mathf.Lerp(finalPitch, pitch, Time.deltaTime * lerpSpeed);
            finalRoll = Mathf.Lerp(finalRoll, roll, Time.deltaTime * lerpSpeed);

            // Calculating drone rotation of the visual part
            Quaternion visualRotation = Quaternion.Euler(finalPitch, 0, finalRoll); 

            // Apply Rotation of the visual part
            transform.GetChild(0).localRotation = visualRotation; // Rotating Drone
        }
        #endif

        #region State

        public PlayerState GetState(uint tick)
        {
            PlayerState state = new PlayerState();
            state.SetUp(tick, transform.position, new Vector3(finalPitch, finalRoll, yaw), _rb.linearVelocity);
            
            return state;
        }

        public void ApplyState(PlayerState state, uint tick = 0)
        {
            transform.position = state.Position;
            finalPitch = state.Rotation.x;
            finalRoll = state.Rotation.y;
            yaw = state.Rotation.z;
            
            // Calculate Rotation
            Quaternion visualRotation = Quaternion.Euler(finalPitch, 0, finalRoll); // Calculating drone rotation
            
            // Apply Rotation
            transform.GetChild(0).localRotation = visualRotation; // Rotating Drone
            ApplyYaw(yaw);

            _rb.linearVelocity = state.Velocity;
        }

        #endregion
    }
}