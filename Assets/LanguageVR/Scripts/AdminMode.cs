using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTNU.CarloMarton.VRLanguage
{
    public class AdminMode : MonoBehaviour
    {
        [SerializeField]
        private PlayerDictionary playerDictionary;

        [SerializeField]
        private InGameMenu inGameMenu;

        private bool isAdmin;

        void Start()
        {
            isAdmin = (bool) PhotonNetwork.LocalPlayer.CustomProperties["admin"];

            InitialiseAdmin();
            FillLoadStateList();
        }

        private void InitialiseAdmin()
        {
            if (isAdmin)
            {
                FillDictionary();
            }
        }

        private void FillDictionary()
        {
            print("Filling dictionary with all interactable objects from Resources/InteractableObjects");
            Object[] objects = Resources.LoadAll("InteractableObjects");
        
            foreach (Object ob in objects)
            {
                GameObject item = (GameObject) ob;
                string name = item.GetComponentInChildren<TextMesh>().text;
                playerDictionary.AddItemToDictionary(name, item);
            }
        }

        private void FillLoadStateList()
        {
            List<string> saveFiles = EnvironmentState.GetAllSaveFileNames();

            foreach (string fileName in saveFiles)
            {
                string nameWithoutExtension = fileName.Substring(0, fileName.Length - 4);
                inGameMenu.AddLoadStateEntry(nameWithoutExtension);
            }
        }
    }
}
