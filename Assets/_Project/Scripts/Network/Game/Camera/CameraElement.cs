using Unity.Netcode;

namespace LindoNoxStudio.Network.Game.Camera
{
    public class CameraElement : NetworkBehaviour
    {
        #if Client
        public override void OnNetworkSpawn()
        {
            Register();
        }

        public override void OnNetworkDespawn()
        {
            RemoveRegistration();
        }

        /// <summary>
        /// Adding this camera element to list
        /// </summary>
        private void Register()
        {
            CameraManager.Instance.AddCameraElement(this, IsOwner);
        }
        
        /// <summary>
        /// Removing this camera element from list
        /// </summary>
        private void RemoveRegistration()
        {
            CameraManager.Instance.RemoveCameraElement(this);
        }
        #endif
    }
}