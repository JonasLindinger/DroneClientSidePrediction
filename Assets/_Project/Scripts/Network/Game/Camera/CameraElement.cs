using Unity.Netcode;

namespace LindoNoxStudio.Network.Game.Camera
{
    #if Client
    public class CameraElement : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            Register();
        }

        public override void OnNetworkDespawn()
        {
            RemoveRegistration();
        }

        private void Register()
        {
            CameraManager.Instance.AddCameraElement(this, IsOwner);
        }
        
        private void RemoveRegistration()
        {
            CameraManager.Instance.RemoveCameraElement(this);
        }
    }
    #endif
}