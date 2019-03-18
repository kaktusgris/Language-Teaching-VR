using NTNU.CarloMarton.VRLanguage;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuUI : MonoBehaviour
{
    private bool visible = true;

    [SerializeField] [Tooltip ("If this element should only be available to admin users")]
    private bool onlyAdmin = false;

    [SerializeField]
    private Color normalColor = Color.black;
    [SerializeField]
    private Color highlightedColor = Color.white;
    [SerializeField]
    private Color pressedColor = Color.green;

    private GameObject playerAvatar;
    private InGameMenu inGameMenu;

    private void Start()
    {
        if (onlyAdmin)
        {
            gameObject.SetActive((bool) PhotonNetwork.LocalPlayer.CustomProperties["admin"]);
        }

        playerAvatar = GameManager.instance.GetPlayerAvatar();
        if (playerAvatar == null)
        {
            playerAvatar = TutorialGameManager.instance.GetPlayerAvatar();
        }
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


    public void OnDeleteObjectToggleClicked()
    {
        visible = !visible;
        transform.Find("DeleteDefaultImage").gameObject.SetActive(visible);
        transform.Find("DeleteActiveImage").gameObject.SetActive(!visible);

        MenuLaser menuLaser = playerAvatar.GetComponent<InGameMenu>().GetLaserHand().GetComponent<MenuLaser>();
        menuLaser.toggleDeleteMode(!visible);
    }

    public void resetDeleteObjectMode()
    {
        if (transform.Find("DeleteActiveImage").gameObject.activeSelf)
        {
            OnDeleteObjectToggleClicked();
        }
    }

    public void OnPlayAudioButtonClicked()
    {
        string objectName = gameObject.GetComponentInParent<Text>().text;
        
        playerAvatar.GetComponent<PlayerDictionary>().PlayAudio(objectName);
    }

    // Spawns the object between the button and the user's head
    public void OnAddInteractableObjectButtonClicked()
    {
        string objectName = gameObject.GetComponentInParent<Text>().text;
        Vector3 headPosition = ViveManager.Instance.head.transform.position;
        Vector3 buttonPosition = transform.position;

        float spawnX = (headPosition.x + buttonPosition.x) / 2f;
        float spawnY = (headPosition.y + buttonPosition.y) / 2f;
        float spawnZ = (headPosition.z + buttonPosition.z) / 2f;

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, spawnZ);

        Debug.LogFormat("Instantiated {0} at {1}", objectName, spawnPosition);
        GameObject interactableObject = PhotonNetwork.Instantiate("InteractableObjects/" + objectName, spawnPosition, Quaternion.identity);
        interactableObject.name = objectName;
    }

    public void OnSaveEnvironmentStateButtonClicked()
    {
        //string timeNow = System.DateTime.Now.ToString("hh.mm dd.MM.yy");
        //string stateName = SceneManagerHelper.ActiveSceneName + " " + timeNow;
        string stateName = SceneManagerHelper.ActiveSceneName + ", " + PhotonNetwork.LocalPlayer.NickName;
        string fileName = EnvironmentState.SaveEnvironmentState(stateName);
        inGameMenu.AddLoadStateEntry(fileName);
        inGameMenu.SetStateSavedName(fileName);
        inGameMenu.TogglePanel(inGameMenu.stateSavedPanel);
    }

    public void OnLoadEnvironmentStateButtonClicked()
    {
        string stateName = transform.parent.GetComponentInChildren<Text>().text;
        EnvironmentState.LoadEnvironmentState(stateName);
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
        if (GameManager.instance.GetPlayerAvatar() != null)
        {
            GameManager.instance.LeaveRoom();
        }
        else
        {
            TutorialGameManager.instance.ExitTutorial();
        }
    }
}
