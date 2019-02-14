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

    private void Start()
    {
        if (onlyAdmin)
        {
            gameObject.SetActive((bool) PhotonNetwork.LocalPlayer.CustomProperties["admin"]);
        }
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
        PlayerManager playerManager = GameObject.Find("GameManager").GetComponent<GameManager>().GetPlayer().GetComponent<PlayerManager>();
        playerManager.SetVisibility(visible);
    }

    public void OnPlayAudioButtonClicked()
    {
        string objectName = gameObject.GetComponentInParent<Text>().text;
        PlayerDictionary dict = GameObject.Find("GameManager").GetComponent<GameManager>().GetPlayer().GetComponent<PlayerDictionary>();
        GameObject interactable = dict.getInteractable(objectName);

        print("Playing audio from " + objectName);
        if (interactable.GetComponent<AudioSource>())
        {
            AudioSource source = interactable.GetComponent<AudioSource>();
            source.Play();
        }
    }

    public void OnAddInteractableObjectButtonClicked()
    {
        string objectName = gameObject.GetComponentInParent<Text>().text;
        Vector3 headPosition = ViveManager.Instance.head.transform.position;
        Vector3 spawnPosition = new Vector3(headPosition.x, headPosition.y, headPosition.z + 0.5f);

        PhotonNetwork.Instantiate(objectName, spawnPosition, Quaternion.identity);
    }
}
