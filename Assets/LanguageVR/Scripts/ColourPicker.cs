using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NTNU.CarloMarton.VRLanguage
{
    public class ColourPicker : MonoBehaviour
    {
        public Image imageResult;
        public bool inLobby = false;

        private RandomColour randomColour;
        private System.Random rng = new System.Random();

        private void Start()
        {
            if (inLobby)
            {
                foreach (InGameMenuUI ui in gameObject.GetComponentsInChildren<InGameMenuUI>())
                {
                    ui.enabled = false;
                }
            }
            else
            {
                GameObject playerAvatar = GameManager.instance.GetPlayerAvatar();
                if (playerAvatar == null)
                    playerAvatar = TutorialGameManager.instance.GetPlayerAvatar();

                randomColour = playerAvatar.GetComponent<RandomColour>();

            }
        }

        private void Update()
        {
            UpdateAvailableColors();
        }

        public void InitialiseColor()
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

            foreach (Transform colorTransform in gameObject.transform)
            {
                Color usedColor = colorTransform.GetComponent<Image>().color;
                if (!takenColors.Contains(usedColor))
                {
                    availableColors.Add(usedColor);
                }
            }

            Color color = availableColors[rng.Next(availableColors.Count)];
            ChangeColor(imageResult, color, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        public void OnPlayerColorClicked()
        {
            //UpdateAvailableColors();
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void OnColorPickerColorClicked(Image img)
        {
            Color color = img.color;
            string colorName = img.gameObject.name;
            if (!gameObject.transform.Find(colorName).Find("DisabledImage").gameObject.activeSelf)
            {
                ChangeColor(imageResult, color, PhotonNetwork.LocalPlayer.ActorNumber);
                gameObject.SetActive(!inLobby);
            }
        }

        public void ChangeColor(Image image, Color color, int ownerId)
        {
            image.color = color;
            float[] colorAsList = { color.r, color.g, color.b, color.a };
            if (PhotonNetwork.LocalPlayer.ActorNumber == ownerId)
            {
                Hashtable hash = new Hashtable() { { "Color", colorAsList } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            }
            if (randomColour != null)
                randomColour.UpdateColour(color);

            //UpdateAvailableColors();
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

            foreach (Transform colorTransform in gameObject.transform)
            {
                Color usedColor = colorTransform.GetComponent<Image>().color;
                GameObject disabledImageObject = colorTransform.Find("DisabledImage").gameObject;
                disabledImageObject.SetActive(takenColors.Contains(usedColor));
            }
        }

        public Color GetColorOnPlayer(Player p)
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