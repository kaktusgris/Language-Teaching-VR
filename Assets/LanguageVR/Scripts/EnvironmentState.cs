using Photon.Pun;
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

        private static readonly string statesSavePath = Application.persistentDataPath + "/EnvironmentStateSaves/";

        public static string SaveEnvironmentState(string sceneName, string saveName)
        {
            string savePath = statesSavePath + sceneName + "/";
            BinaryFormatter bf = new BinaryFormatter();

            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            string fileName = GetValidFileName(savePath, saveName, ".dat");
            string filePath = savePath + fileName + ".dat";
            FileStream file = File.Open(filePath, FileMode.Create, FileAccess.Write);

            EnvironmentInfo environmentInfo = new EnvironmentInfo();
            List<GameObject> interactables = GetAllInteractableGameObjectsInScene();
            environmentInfo.interactables = GetNamesFromGameObjects(interactables);
            environmentInfo.positions = GetAllInteractablePositions(interactables);
            environmentInfo.rotations = GetAllInteractableRotations(interactables);

            bf.Serialize(file, environmentInfo);
            file.Close();
            Debug.LogFormat("State saved at {0}", filePath);

            return fileName;
        }

        public static void LoadEnvironmentState(string sceneName, string saveName)
        {
            string filePath = statesSavePath + sceneName + "/" + saveName + ".dat";
            if (File.Exists(filePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(filePath, FileMode.Open, FileAccess.Read);
                EnvironmentInfo info = (EnvironmentInfo)bf.Deserialize(file);
                file.Close();

                DestroyAllInteractableObjectsInScene();
                SpawnAllInteractableObjects(info);

                Debug.LogFormat("Loaded interactable objects from {0}", filePath);
            }
            else
            {
                Debug.LogErrorFormat("Could not load file at {0}. Does not exist.", filePath);
            }
        }

        public static List<string> GetAllSaveFileNames(string sceneName)
        {
            string savePath = statesSavePath + sceneName + "/";
            if (!Directory.Exists(savePath))
                return new List<string>();

            DirectoryInfo d = new DirectoryInfo(@savePath);
            FileInfo[] files = d.GetFiles("*.dat"); //Getting Text files
            List<string> filenames = new List<string>();
            foreach (FileInfo file in files)
            {
                filenames.Add(file.Name);
            }
            return filenames;
        }

        private static void DestroyAllInteractableObjectsInScene()
        {
            foreach (GameObject go in GetAllInteractableGameObjectsInScene())
            {
                GameManager.instance.DestroySomething(go);
                //MonoBehaviour mb = new MonoBehaviour();
                //mb.StartCoroutine(DestroyInteractableGameObject(go));
            }
        }

        private static string GetValidFileName(string path, string name, string extension)
        {
            string fileName = name;
            int counter = 0;

            while (File.Exists(path + fileName + extension))
            {
                counter++;
                fileName = name + " (" + counter + ")";
            }
            return fileName;
        }

        private static void SpawnAllInteractableObjects(EnvironmentInfo info)
        {
            for (int i = 0; i < info.Count(); i++)
            {
                string name = info.interactables[i];
                Vector3 position = FloatToVector3(info.positions[i]);
                Quaternion rotation = FloatToQuaternion(info.rotations[i]);
                GameObject go = PhotonNetwork.Instantiate("InteractableObjects/" + name, position, rotation);
                go.name = name;
            }
        }

        private static List<GameObject> GetAllInteractableGameObjectsInScene()
        {
            List<GameObject> interactables = new List<GameObject>();

            foreach (GameObject interactable in GameObject.FindGameObjectsWithTag("InteractableObject"))
            {
                interactables.Add(interactable);
            }

            return interactables;
        }

        private static List<string> GetNamesFromGameObjects(List<GameObject> interactables)
        {
            List<string> gameObjectNames = new List<string>();

            foreach (GameObject interactable in GameObject.FindGameObjectsWithTag("InteractableObject"))
            {
                if (interactable.name.Contains("(Clone)"))
                {
                    int substringLength = interactable.name.Length - "(Clone)".Length;
                    gameObjectNames.Add(interactable.name.Substring(0, substringLength));
                }
                else
                {
                    gameObjectNames.Add(interactable.name);
                }
            }

            return gameObjectNames;
        }

        private static List<float[]> GetAllInteractablePositions(List<GameObject> interactables)
        {
            List<float[]> positions = new List<float[]>();

            foreach (GameObject interactable in interactables)
            {
                Vector3 vector = interactable.transform.position;
                float[] postition = { vector.x, vector.y, vector.z };
                positions.Add(postition);
            }

            return positions;
        }

        private static List<float[]> GetAllInteractableRotations(List<GameObject> interactables)
        {
            List<float[]> rotations = new List<float[]>();

            foreach (GameObject interactable in interactables)
            {
                Vector3 quaternion = interactable.transform.rotation.eulerAngles;
                float[] rotation = { quaternion.x, quaternion.y, quaternion.z };
                rotations.Add(rotation);
            }

            return rotations;
        }

        private static Vector3 FloatToVector3(float[] floats)
        {
            return new Vector3(floats[0], floats[1], floats[2]);
        }

        private static Quaternion FloatToQuaternion(float[] floats)
        {
            return Quaternion.Euler(floats[0], floats[1], floats[2]);
        }
    }

    [Serializable]
    class EnvironmentInfo
    {
        public List<String> interactables = new List<String>();
        public List<float[]> positions = new List<float[]>();
        public List<float[]> rotations = new List<float[]>();

        public int Count()
        {
            return interactables.Count;
        }
    }
}
