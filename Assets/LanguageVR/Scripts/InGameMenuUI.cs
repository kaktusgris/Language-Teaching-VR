﻿using ExitGames.Client.Photon;
using NTNU.CarloMarton.VRLanguage;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// --------
// Script attached to each button and other relevant element in the InGameMenu
// --------
public class InGameMenuUI : MonoBehaviour, IObserver<VoiceRecognitionStatus>
{
    private bool visible = true;
    private VoiceRecognitionStatus value;

    [SerializeField] [Tooltip ("If this element should only be available to admin users")]
    private bool onlyAdmin = false;

    [SerializeField]
    [Tooltip("")]
    private bool externalButton = false;

    [SerializeField]
    private Color normalColor = Color.black;
    [SerializeField]
    private Color highlightedColor = Color.white;
    [SerializeField]
    private Color pressedColor = Color.green;

    [Header("Voice Recognition Settings")]
    public int voiceRecDurationInSeconds = 20;


    private int voiceRecTimer;
    private GameObject playerAvatar;
    private InGameMenu inGameMenu;

    private void Start()
    {
        if (onlyAdmin)
        {
            gameObject.SetActive((bool) PhotonNetwork.LocalPlayer.CustomProperties["admin"]);
        }

        if (GameManager.instance != null)
            playerAvatar = GameManager.instance.GetPlayerAvatar();
        else
            playerAvatar = TutorialGameManager.instance.GetPlayerAvatar();

        if (!externalButton)
            inGameMenu = playerAvatar.GetComponent<InGameMenu>();
    }

    public Color GetNormalColor()
    {
        return normalColor;
    }

    public Color GetHighlightedColor()
    {
        return highlightedColor;
    }

    public Color GetPressedColor()
    {
        return pressedColor;
    }

    public void OnInvisibilityToggleClicked()
    {
        visible = !visible;
        transform.Find("VisibleImage").gameObject.SetActive(visible);
        transform.Find("InvisibleImage").gameObject.SetActive(!visible);

        playerAvatar.GetComponent<PlayerManager>().SetVisibility(visible);
    }

	// Add ResetEditObjectMode to same button to run on EditObjectButton
    public void OnDeleteObjectToggleClicked()
    {
        visible = !visible;
        transform.Find("DeleteDefaultImage").gameObject.SetActive(visible);
        transform.Find("DeleteActiveImage").gameObject.SetActive(!visible);

        MenuLaser menuLaser = playerAvatar.GetComponent<InGameMenu>().GetLaserHand().GetComponent<MenuLaser>();
        menuLaser.ToggleDeleteMode(!visible);
	}

	// Add ResetDeleteObjectMode to same button to run on DeleteObjectButton
	public void OnEditObjectToggleClicked()
	{
		visible = !visible;
		transform.GetChild(0).GetComponent<Image>().color = visible ? new Color(93f/255f, 93f/255f, 93f/255f) : Color.blue;

		MenuLaser menuLaser = playerAvatar.GetComponent<InGameMenu>().GetLaserHand().GetComponent<MenuLaser>();
		menuLaser.ToggleEditMode(!visible);
	}

	public void ResetEditObjectMode()
	{
		visible = true;
		transform.GetChild(0).GetComponent<Image>().color = new Color(93f / 255f, 93f / 255f, 93f / 255f);
	}

	public void ResetDeleteObjectMode()
	{
		visible = true;
		transform.Find("DeleteDefaultImage").gameObject.SetActive(true);
		transform.Find("DeleteActiveImage").gameObject.SetActive(false);
	}

    public void OnPlayAudioButtonClicked()
    {
        string objectName = transform.parent.GetComponentInChildren<Text>().text;

        playerAvatar.GetComponent<PlayerDictionary>().PlayAudio(objectName);
    }

    // Spawns the object between the button and the user's head
    public void OnAddInteractableObjectButtonClicked()
    {
        string objectName = transform.parent.GetComponentInChildren<Text>().text;
        Vector3 headPosition = ViveManager.Instance.head.transform.position;
        Vector3 buttonPosition = transform.position;

        float spawnX = (headPosition.x + buttonPosition.x) / 2f;
        float spawnY = (headPosition.y + buttonPosition.y) / 2f;
        float spawnZ = (headPosition.z + buttonPosition.z) / 2f;

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, spawnZ);

        Debug.LogFormat("Instantiated {0} at {1}", objectName, spawnPosition);
        GameObject interactableObject = PhotonNetwork.Instantiate("InteractableObjects/" + objectName, spawnPosition, Quaternion.identity);
        //interactableObject.name = objectName;
    }

    public void OnVoiceRecognitionButtonClicked()
    {
        string objectName = transform.parent.GetComponentInChildren<Text>().text;
        inGameMenu.voiceRecDurationSlider.maxValue = voiceRecDurationInSeconds;
        inGameMenu.voiceRecDurationSlider.value = voiceRecDurationInSeconds;
        inGameMenu.SetPanelActive(inGameMenu.voiceRecognitionPanel.name);

        inGameMenu.voiceRecognitionWordText.text = "Stemmegjenkjenning startet, si ordet:  " + objectName;

        VoiceRecognitionScript.InitiateSpeechRecognition(voiceRecDurationInSeconds, objectName);
        VoiceRecognitionScript.instance.Subscribe(this);
    }

    public void OnVoiceRecognitionCancelButtonClicked()
    {
        VoiceRecognitionScript.CancelSpeechRecognition();
        inGameMenu.SetPanelActive(inGameMenu.dictionaryPanel.name);
    }

    public void OnSaveEnvironmentStateButtonClicked()
    {
        //string timeNow = System.DateTime.Now.ToString("hh.mm dd.MM.yy");
        //string stateName = SceneManagerHelper.ActiveSceneName + " " + timeNow;
        string sceneName = SceneManagerHelper.ActiveSceneName;
        string stateName;
        if (PhotonNetwork.IsConnected)
            stateName = PhotonNetwork.CurrentRoom.Name + ", " + PhotonNetwork.LocalPlayer.NickName;
        else
            stateName = "Offline, Anon";

        string fileName = EnvironmentState.SaveEnvironmentState(sceneName, stateName);
        inGameMenu.AddLoadStateEntry(fileName);
        inGameMenu.SetStateSavedName(fileName);
        inGameMenu.TogglePanel(inGameMenu.stateSavedPanel);
    }

    public void OnLoadEnvironmentStateButtonClicked()
    {
        string sceneName = SceneManagerHelper.ActiveSceneName;
        string stateName = transform.parent.GetComponentInChildren<Text>().text;
        EnvironmentState.LoadEnvironmentState(sceneName, stateName);
    }

    public void OnDeleteStateButtonClicked()
    {
        string sceneName = SceneManagerHelper.ActiveSceneName;
        string stateName = transform.parent.GetComponentInChildren<Text>().gameObject.name;
        EnvironmentState.DeleteSaveFile(sceneName, stateName);
        inGameMenu.DeleteLoadStateEntry();
        inGameMenu.TogglePanel(inGameMenu.loadStatePanel);
    }

    public void OnDeleteStatePanelButtonClicked()
    {
        string stateName = transform.parent.GetComponentInChildren<Text>().text;
        inGameMenu.SetEntryToBeDeleted(transform.parent.gameObject);
        inGameMenu.SetStateDeleteName(stateName);
        inGameMenu.TogglePanel(inGameMenu.deleteStatePanel);
    }

	public void OnToggleObjectTextButtonClicked()
	{
		string textVisible = "Skru av objekttekst";
		string textHidden = "Skru på objekttekst";

		bool showObjectText = (bool)PhotonNetwork.LocalPlayer.CustomProperties["ShowObjectText"];

		transform.GetComponentInChildren<Text>().text = showObjectText ? textHidden : textVisible;
		foreach (Player player in PhotonNetwork.PlayerList) {
			player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "ShowObjectText", !showObjectText } });
		}

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "ShowObjectText", !showObjectText } });
    }

    public void OnToggleSettingsPanelButtonClicked()
    {
        inGameMenu.TogglePanel(inGameMenu.settingsPanel);
    }

    public void OnToggleLoadStatePanelButtonClicked()
    {
        inGameMenu.TogglePanel(inGameMenu.loadStatePanel);
    }

    public void OnToggleChangeColorPanelButtonClicked()
    {
        Transform disabledImageTransform = transform.Find("DisabledImage");
        if (disabledImageTransform == null)
        {
            inGameMenu.TogglePanel(inGameMenu.changeColorPanel);
        }
        else if (!disabledImageTransform.gameObject.activeSelf)
        {
            inGameMenu.TogglePanel(inGameMenu.changeColorPanel, inGameMenu.settingsPanel);
        }
    }

    public void OnToggleExitGamePanelButtonClicked()
    {
        inGameMenu.TogglePanel(inGameMenu.exitGamePanel);
    }

    public void OnExitGameButtonClicked()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.LeaveRoom();
        }
        else
        {
            TutorialGameManager.instance.ExitTutorial();
        }
    }

    public void OnCompleted()
    {
        if (value != null && value.Status)
        {
            Debug.Log("Result is: " + value.Status);
        }

        inGameMenu.SetPanelActive("DictionaryPanel");
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(VoiceRecognitionStatus value)
    {
        this.value = value;
    }

}
