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
        public Button PlayerReadyButton;
        public Image PlayerReadyImage;

        public Button ColorPickerPutton;
        public GameObject ColorPicker;

        private int ownerId;
        private bool isPlayerReady;

        private System.Random rng = new System.Random();

        #region UNITY

        public void Start()
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId)
            {
                PlayerReadyButton.gameObject.SetActive(false);
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
                ColorPickerPutton.onClick.AddListener(OnPlayerColorClicked);


                InitialiseColor();
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
                        Color playerColor = GetColorOnPlayer(p);
                        // Only change if the colour is different
                        if (!playerColor.Equals(PlayerColorImage.color))
                        {
                            ChangeColor(playerColor);
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

        private void InitialiseColor()
        {
            List<Color> availableColors = new List<Color>();
            List<Color> takenColors = new List<Color>();
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                object colorObject = GetColorOnPlayer(player);
                if (colorObject != null)
                {
                    takenColors.Add((Color)colorObject);
                }
            }

            foreach (Transform colorTransform in ColorPicker.transform)
            {
                Color usedColor = colorTransform.GetComponent<Image>().color;
                if (!takenColors.Contains(usedColor))
                {
                    availableColors.Add(usedColor);
                }
            }

            Color color = availableColors[rng.Next(availableColors.Count)];
            ChangeColor(color);
        }

        public void OnPlayerColorClicked()
        {
            UpdateAvailableColors();
            ColorPicker.SetActive(!ColorPicker.activeSelf);
        }

        public void OnColorPickerColorClicked(Image img)
        {
            Color color = img.color;
            string colorName = img.gameObject.name;
            print("Changed player colour to " + colorName);
            if (!ColorPicker.transform.Find(colorName).Find("DisabledImage").gameObject.activeSelf)
            {
                ChangeColor(color);
                ColorPicker.SetActive(false);
            }
        }

        private void ChangeColor(Color color)
        {
            PlayerColorImage.color = color;
            float[] colorAsList = { color.r, color.g, color.b, color.a };
            if (PhotonNetwork.LocalPlayer.ActorNumber == ownerId)
            {
                Hashtable hash = new Hashtable() { { "Color", colorAsList } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            }
            UpdateAvailableColors();
        }

        private void UpdateAvailableColors()
        {
            List<Color> takenColors = new List<Color>();
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                Color colorObject = GetColorOnPlayer(player);
                if (colorObject != null)
                {
                    takenColors.Add(colorObject);
                }
            }

            foreach (Transform colorTransform in ColorPicker.transform)
            {
                Color usedColor = colorTransform.GetComponent<Image>().color;
                GameObject disabledImageObject = colorTransform.Find("DisabledImage").gameObject;
                disabledImageObject.SetActive(takenColors.Contains(usedColor));
            }
        }

        private Color GetColorOnPlayer(Player p)
        {
            float[] colorAsFloat = (float[])p.CustomProperties["Color"];
            if (colorAsFloat == null)
            {
                return Color.black;
            }

            float r = colorAsFloat[0];
            float g = colorAsFloat[1];
            float b = colorAsFloat[2];
            float a = colorAsFloat[3];

            return new Color(r, g, b, a);
        }
    }
}