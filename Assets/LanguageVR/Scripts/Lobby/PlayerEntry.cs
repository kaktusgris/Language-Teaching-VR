using UnityEngine;
using UnityEngine.UI;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;
using System.Collections.Generic;

namespace NTNU.CarloMarton.VRLanguage
{
    public class PlayerEntry : MonoBehaviour
    {
        [Header("UI References")]
        public Text PlayerNameText;

        public Image PlayerColorImage;
        public Image DropdownImage;
        public Button PlayerReadyButton;
        public Image PlayerReadyImage;

        public ColourPicker colourPicker;

        private int ownerId;
        private bool isPlayerReady;

        #region UNITY

        public void Start()
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId)
            {
                PlayerReadyButton.gameObject.SetActive(false);
                DropdownImage.gameObject.SetActive(false);
            }
            else
            {
                PlayerReadyButton.onClick.AddListener(() =>
                {
                    isPlayerReady = !isPlayerReady;
                    SetPlayerReady(isPlayerReady);

                    Hashtable props = new Hashtable() { { "IsPlayerReady", isPlayerReady } };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                    if (PhotonNetwork.IsMasterClient)
                    {
                        FindObjectOfType<MainPanel>().LocalPlayerPropertiesUpdated();
                    }
                });
                colourPicker.InitialiseColor();
            }
        }

        private void Update()
        {
            // Only need to change colour on other players tag, your own is handled on change
            if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId)
            {
                foreach (Player p in PhotonNetwork.PlayerListOthers)
                {
                    // Find the player owning this entry
                    if (p.ActorNumber == ownerId)
                    {
                        Color playerColor = colourPicker.GetColorOnPlayer(p);
                        // Only change if the colour is different
                        if (!playerColor.Equals(PlayerColorImage.color))
                        {
                            colourPicker.ChangeColor(PlayerColorImage, playerColor, ownerId);
                        }
                    }
                }
            }
        }

        #endregion

        public void Initialize(int playerId, string playerName)
        {
            ownerId = playerId;
            PlayerNameText.text = playerName;
        }

        public void SetPlayerReady(bool playerReady)
        {
            PlayerReadyButton.GetComponentInChildren<Text>().text = playerReady ? "Klar!" : "Klar?";
            PlayerReadyImage.enabled = playerReady;
        }

       
    }
}