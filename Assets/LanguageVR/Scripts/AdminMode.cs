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
        }

        private void InitialiseAdmin()
        {
            if (isAdmin)
            {
                FillDictionary();
                FillLoadStateList();
            }
        }

        private void FillDictionary()
        {
            print("Filling dictionary with all interactable objects from Resources/InteractableObjects");
            Object[] objects = Resources.LoadAll("InteractableObjects");
            string[] objectNames = new string[objects.Length];
            Dictionary<string, GameObject> nameObjectPairs = new Dictionary<string, GameObject>();

            for (int i = 0; i < objects.Length; i++)
            {
                GameObject item = (GameObject) objects[i];
                string name = item.GetComponentInChildren<TextMesh>().text;
                objectNames[i] = name;
                nameObjectPairs.Add(name, item);
            }

            System.Array.Sort(objectNames);
            foreach (string name in objectNames)
            {
                playerDictionary.AddItemToDictionary(name, nameObjectPairs[name]);
            }
        }

        private void FillLoadStateList()
        {
            List<string> saveFiles = EnvironmentState.GetAllSaveFileNames(SceneManagerHelper.ActiveSceneName);
            saveFiles.Sort();

            foreach (string fileName in saveFiles)
            {
                string nameWithoutExtension = fileName.Substring(0, fileName.Length - 4);
                inGameMenu.AddLoadStateEntry(nameWithoutExtension);
            }
        }
    }
}
