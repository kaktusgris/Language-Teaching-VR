using ExitGames.Client.Photon;
using NTNU.CarloMarton.VRLanguage;
using Photon.Pun;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class InGameMenu : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean menuButtonAction;

    public GameObject menu;
    public GameObject helperUI;

    [Header("Panels")]
    public GameObject dictionaryPanel;
    public GameObject settingsPanel;
    public GameObject stateSavedPanel;
    public GameObject loadStatePanel;
    public GameObject deleteStatePanel;
    public GameObject changeColorPanel;
    public GameObject exitGamePanel;
    private GameObject defaultPanel;


    [Header("Entry prefabs")]
    public GameObject menuObjectEntry;
    public GameObject loadStateEntry;

    [Header("Menu elements")]
    public Image currentColorImage;
    public Text topPanelTitle;

    private GameObject leftHand;
    private GameObject rightHand;
    private SteamVR_Input_Sources currentHandParent;
    private GameObject laserHand;
    private GameObject handPrefabL;
    private GameObject handPrefabR;

    private GameObject entryToDelete;

    private void Awake()
    {
        defaultPanel = dictionaryPanel; // Set which panel will be the default
        SetPanelActive(defaultPanel.name);

        // Force admin mode on user if not set before. Most likely cause it was launched in editor. Neccessary to be able to play straight in scene
        if (PhotonNetwork.LocalPlayer.CustomProperties["admin"] == null)
        {
            print("User set as admin");
            Hashtable hash = new Hashtable();
            hash.Add("admin", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

        leftHand = ViveManager.Instance.leftHand;
        rightHand = ViveManager.Instance.rightHand;

        handPrefabL = transform.Find("HandPrefabL").gameObject;
        handPrefabR = transform.Find("HandPrefabR").gameObject;

        currentColorImage.color = gameObject.GetComponent<RandomColour>().colour;
    }

    private void FixActiveMenu(SteamVR_Input_Sources hand)
    {
        // Don't do anything if it is not your own menu
        if (!gameObject.GetComponent<PhotonView>().IsMine && PhotonNetwork.IsConnected)
            return;

        // Only deactivate the menu if the same button is pressed twice, swap hand if not
        if (menu.activeInHierarchy && hand.Equals(currentHandParent))
        {
            SetEnablelaserOnHand(handPrefabR, false);
            SetEnablelaserOnHand(handPrefabL, false);
            menu.SetActive(false);
            helperUI.SetActive(false);
        }
        else
        {
            if (hand == SteamVR_Input_Sources.RightHand)
            {
                currentHandParent = SteamVR_Input_Sources.RightHand;
                menu.transform.SetParent(rightHand.transform);
                helperUI.transform.SetParent(leftHand.transform);

                laserHand = handPrefabL;
                SetEnablelaserOnHand(handPrefabL, true);
                SetEnablelaserOnHand(handPrefabR, false);
            }
            else if (hand == SteamVR_Input_Sources.LeftHand)
            {
                currentHandParent = SteamVR_Input_Sources.LeftHand;
                menu.transform.SetParent(leftHand.transform);
                helperUI.transform.SetParent(rightHand.transform);

                laserHand = handPrefabR;
                SetEnablelaserOnHand(handPrefabR, true);
                SetEnablelaserOnHand(handPrefabL, false);
            }
            else
            {
                Debug.LogError("Must be attached to either left hand or right hand");
            }

            menu.transform.localPosition = new Vector3(0, -0.25f, 0.25f);
            menu.transform.localRotation = Quaternion.Euler(60, 0, 0);
            menu.transform.Find("TopPanel").transform.Find("DeleteObjectButton").GetComponent<InGameMenuUI>().ResetDeleteObjectMode();
			menu.transform.Find("TopPanel/EditObjectButton").GetComponent<InGameMenuUI>().ResetEditObjectMode();
            menu.SetActive(true);

            helperUI.transform.localPosition = new Vector3(0, 0, 0.01f);
            helperUI.transform.localRotation = Quaternion.Euler(60, 0, 0);
            helperUI.SetActive(true);

            SetPanelActive(defaultPanel.name);
        }
    }

    public void TogglePanel(GameObject panel)
    {
        TogglePanel(panel, defaultPanel);
    }

    public void TogglePanel(GameObject panel, GameObject otherPanel)
    {
        if (panel.activeInHierarchy)
            SetPanelActive(otherPanel.name);
        else
            SetPanelActive(panel.name);
    }

    public void SetPanelActive(string panelName)
    {
        SetTitleOnTopPanel(panelName);

        dictionaryPanel.SetActive(dictionaryPanel.name.Equals(panelName));
        exitGamePanel.SetActive(exitGamePanel.name.Equals(panelName));
        loadStatePanel.SetActive(loadStatePanel.name.Equals(panelName));
        settingsPanel.SetActive(settingsPanel.name.Equals(panelName));
        changeColorPanel.SetActive(changeColorPanel.name.Equals(panelName));
        stateSavedPanel.SetActive(stateSavedPanel.name.Equals(panelName));
        deleteStatePanel.SetActive(deleteStatePanel.name.Equals(panelName));
    }

    public void SetTitleOnTopPanel(string panelName)
    {
        if (panelName.Equals(dictionaryPanel.name))
            topPanelTitle.text = "Ordbok";
        else if (panelName.Equals(exitGamePanel.name))
            topPanelTitle.text = "Avslutt";
        else if (panelName.Equals(settingsPanel.name))
            topPanelTitle.text = "Instillinger";
        else if (panelName.Equals(loadStatePanel.name))
            topPanelTitle.text = "Last inn";
        else if (panelName.Equals(changeColorPanel.name))
            topPanelTitle.text = "Endre farge";
    }

    public void SetStateSavedName(string stateName)
    {
        string text = "Rom lagret som " + stateName;
        stateSavedPanel.GetComponentInChildren<Text>().text = text;
    }

    public void SetStateDeleteName(string stateName)
    {
        string text = "Er du sikker på at du vil slette " + stateName + "?";
        deleteStatePanel.GetComponentInChildren<Text>().gameObject.name = stateName;
        deleteStatePanel.GetComponentInChildren<Text>().text = text;
    }

    private void SetEnablelaserOnHand(GameObject hand, bool enabled)
    {
        if (gameObject.GetComponent<PhotonView>().IsMine || !PhotonNetwork.IsConnected)
        {
            LineRenderer lr = hand.GetComponent<LineRenderer>();
            MenuLaser menuLaser = hand.GetComponent<MenuLaser>();
            menuLaser.ToggleDeleteMode(false);
            lr.enabled = enabled;
            menuLaser.enabled = enabled;
        }
    }

    public GameObject GetLaserHand()
    {
        return laserHand;
    }

    public void AddMenuObjectEntry(string text)
    {
        GameObject newEntry = (GameObject)Instantiate(menuObjectEntry, dictionaryPanel.transform.Find("ScrollContent"));
        newEntry.GetComponentInChildren<Text>().text = text;
    }

    public void AddLoadStateEntry(string name)
    {
        GameObject newEntry = (GameObject)Instantiate(loadStateEntry, loadStatePanel.transform.Find("ScrollContent"));
        newEntry.GetComponentInChildren<Text>().text = name;
    }

    public void SetEntryToBeDeleted(GameObject entry)
    {
        entryToDelete = entry;
    }

    public void DeleteLoadStateEntry()
    {
        GameObject.Destroy(entryToDelete);
    }

    private void Update()
    {
        if (menuButtonAction.GetStateDown(handType))
        {
            if (menuButtonAction.GetStateDown(SteamVR_Input_Sources.LeftHand))
            {
                FixActiveMenu(SteamVR_Input_Sources.LeftHand);
            }
            else if (menuButtonAction.GetStateDown(SteamVR_Input_Sources.RightHand)) {
                FixActiveMenu(SteamVR_Input_Sources.RightHand);
            }
        }
    }
}
