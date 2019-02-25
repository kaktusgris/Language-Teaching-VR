using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace NTNU.CarloMarton.VRLanguage
{
    public class TopPanel : MonoBehaviour
    {
        private readonly string connectionStatusMessage = "    Status: ";

        [Header("UI References")]
        public Text ConnectionStatusText;
        public Text RoleStatusText;

        #region UNITY

        public void Update()
        {
            ConnectionStatusText.text = connectionStatusMessage + PhotonNetwork.NetworkClientState;
        }

        public void UpdateRoleText()
        {
            object adminValue;
            if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("admin", out adminValue))
            {
                RoleStatusText.text = (bool) adminValue ? "Lærer" : "Student";
            }
        }

        #endregion
    }
}