using LindoNoxStudio.Network.Input;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace LindoNoxStudio.Network.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : NetworkBehaviour
    {
        // Values we change in unity
        [Header("Settings")] 
        [SerializeField] private float speed = 8f;
        [SerializeField] private float sensitivity = 4f;
        [Space(10)]
        [Header("Animation")] 
        [SerializeField] private float minMaxPitch = 30f;
        [SerializeField] private float minMaxRoll = 30f;
        [SerializeField] private float lerpSpeed = 2f;
        
        // Values we change in code
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

            // Todo: Apply Rotation of Input
            
            // Applying Force
            _rb.AddForce(GetEngineForce(input), ForceMode.Force);
            
            // Todo: Do Rotation
        }
        
        /// <summary>
        /// Returns the engine Force
        /// </summary>
        /// <param name="input">Input we use for the engine force</param>
        /// <returns></returns>
        private Vector3 GetEngineForce(ClientInputState input)
        {
            Vector3 inputForce = new Vector3(input.GetCycle().x, input.Throttle, input.GetCycle().y).normalized;
            Vector3 gravityCounterForce = Vector3.up * (_rb.mass * Physics.gravity.magnitude);
            Vector3 engineForce =
                //gravityCounterForce + // Counter gravity We don't counter gravity. We just don't enable gravity at all
                (transform.TransformDirection(inputForce) * speed);  // Move Input * Power

            return engineForce;
        }
    }
}