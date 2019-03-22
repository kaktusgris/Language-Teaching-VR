using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NTNU.CarloMarton.VRLanguage
{
    public class StatePicker : MonoBehaviour
    {
        public GameObject content;
        public GameObject entryPrefab;

        private void Start()
        {
        
        }

        private void UpdateStates()
        {
            //List<string> states = EnvironmentState.GetAllSaveFileNames();
        }

        private void AddState()
        {
            GameObject newState = GameObject.Instantiate(entryPrefab);
            entryPrefab.transform.SetParent(content.transform);
        }

        private void ClearStates()
        {

        }
    }
}
