using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTNU.CarloMarton.VRLanguage
{
    public static class EnvironmentState
    {

        public static void SaveEnvironmentState(string path)
        {
            string savepath = Application.persistentDataPath + "/helloWorld.dat";// + "/" + path;

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(savepath, FileMode.Create);

            EnvironmentInfo environmentInfo = new EnvironmentInfo();
            environmentInfo.interactables = GetAllInteractableObjectsInEnvironment();
            environmentInfo.transforms = GetAllInteractableTransforms();

            bf.Serialize(file, environmentInfo);
            file.Close();
            Debug.Log("Saved");

            LoadEnvironmentState();
        }

        private static Dictionary<string, float[]> GetAllInteractableTransforms()
        {
            Dictionary<string, float[]> gameObjects = new Dictionary<string, float[]>();

            foreach (GameObject interactable in GameObject.FindGameObjectsWithTag("InteractableObject"))
            {
                Vector3 vector = interactable.transform.position;
                float[] postition = { vector.x, vector.y, vector.z };
                gameObjects.Add(interactable.name, postition);
            }

            return gameObjects;
        }

        public static void LoadEnvironmentState()
        {
            string filePath = Application.persistentDataPath + "/helloWorld.dat";
            if (File.Exists(filePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(filePath, FileMode.Open);
                EnvironmentInfo info = (EnvironmentInfo)bf.Deserialize(file);
                file.Close();
                Debug.Log(info.interactables[1]);
                Debug.Log(info.transforms[info.interactables[1]][0]);
            }
        }

        private static List<string> GetAllInteractableObjectsInEnvironment()
        {
            List<string> gameObjects = new List<string>();

            foreach (GameObject interactable in GameObject.FindGameObjectsWithTag("InteractableObject"))
            {
                gameObjects.Add(interactable.name);
            }

            return gameObjects;
        }
    }

    [Serializable]
    class EnvironmentInfo
    {
        public List<String> interactables = new List<String>();
        public Dictionary<String, float[]> transforms = new Dictionary<String, float[]>();
    }
}
