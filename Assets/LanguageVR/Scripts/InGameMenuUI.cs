using NTNU.CarloMarton.VRLanguage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuUI : MonoBehaviour
{
    private bool visible = true;

    [SerializeField]
    private Color standbyColor = Color.black;
    [SerializeField]
    private Color hoverColor = Color.white;

    public Color GetHoverColor()
    {
        return hoverColor;
    }

    public Color GetStandbyColor()
    {
        return standbyColor;
    }

    public void OnInvisibilityToggleClicked()
    {
        visible = !visible;
        transform.Find("VisibleImage").gameObject.SetActive(visible);
        transform.Find("InvisibleImage").gameObject.SetActive(!visible);
        PlayerManager playerManager = GameObject.Find("GameManager").GetComponent<GameManager>().GetPlayer().GetComponent<PlayerManager>();
        playerManager.SetVisibility(visible);
    }

    public void OnTextBlockClicked()
    {
        string objectName = gameObject.GetComponent<Text>().text;
        PlayerDictionary dict = GameObject.Find("GameManager").GetComponent<GameManager>().GetPlayer().GetComponent<PlayerDictionary>();
        GameObject interactable = dict.getInteractable(objectName);

        print(objectName);
        if (interactable.GetComponent<AudioSource>())
        {
            AudioSource source = interactable.GetComponent<AudioSource>();
            source.Play();
        }
    }
}
