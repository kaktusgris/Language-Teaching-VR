using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NTNU.CarloMarton.VRLanguage
{
    public class ScenePicker : MonoBehaviour
    {
        public MainPanel mainPanel;
        public Image imageViewer;
        public Text sceneNameViewer;
        public GameObject stateContent;
        public GameObject stateEntryPrefab;

        public List<string> sceneNames;
        public List<Sprite> sceneScreenshots;

        private int currentScene = 0;
        private List<string> states;
        private string stateToLoad;

        private void Start()
        {
            NewScene(currentScene);

            UpdateStateList();
        }

        private void UpdateStateList()
        {
            ClearStates();
            string sceneName = sceneNames[currentScene];
            int numberOfStates = 0;
            foreach (string state in EnvironmentState.GetAllSaveFileNames(sceneName))
            {
                numberOfStates+=7;
                string stateWithoutExtension = state.Substring(0, state.Length - ".dat".Length);
                AddState(stateWithoutExtension);
                AddState(stateWithoutExtension);
                AddState(stateWithoutExtension);
                AddState(stateWithoutExtension);
                AddState(stateWithoutExtension);
                AddState(stateWithoutExtension);
                AddState(stateWithoutExtension);
            }
            UpdateRectSize(numberOfStates);
        }

        private void AddState(string state)
        {
            GameObject stateEntry = GameObject.Instantiate(stateEntryPrefab);
            stateEntry.GetComponentInChildren<Text>().text = state;

            stateEntry.transform.SetParent(stateContent.transform, false);

            stateEntry.GetComponent<Button>().onClick.AddListener(() =>
            {
                print(state);
                mainPanel.SetStateToLoad(state);
                mainPanel.SetActivePanel(mainPanel.CreateRoomPanel.name);
            });
        }

        private void ClearStates()
        {
            for (int i=0; i < stateContent.transform.childCount; i++)
            {
                Destroy(stateContent.transform.GetChild(i).gameObject);
            }
        }

        private string NewScene(int sceneNumber)
        {
            imageViewer.sprite = sceneScreenshots[sceneNumber];
            string name = sceneNames[sceneNumber];
            sceneNameViewer.text = name;
            return name;
        }

        private void UpdateRectSize(int numberOfStates)
        {
            RectTransform rt = stateContent.GetComponent<RectTransform>();
            VerticalLayoutGroup vlg = stateContent.GetComponent<VerticalLayoutGroup>();

            float entryHeight = rt.GetChild(0).GetComponent<RectTransform>().rect.height;
            float spacing = vlg.spacing;
            float padding = vlg.padding.top + vlg.padding.bottom;

            float newHeight = numberOfStates * (entryHeight + spacing) + padding - spacing;
            rt.sizeDelta = new Vector2(rt.rect.width, newHeight);
        }

        public void OnNextButtonClicked()
        {
            currentScene++;
            if (currentScene == sceneNames.Count)
            {
                currentScene = 0;
            }
            NewScene(currentScene);
        }

        public void OnPreviousButtonClicked()
        {
            currentScene--;
            if (currentScene < 0)
            {
                currentScene = sceneNames.Count - 1;
            }
            NewScene(currentScene);
        }

        public void OnOpenLoadStatePanelButtonClicked()
        {
            UpdateStateList();
        }
    }
}
