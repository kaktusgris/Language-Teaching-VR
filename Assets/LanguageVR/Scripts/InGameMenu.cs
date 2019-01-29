using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class InGameMenu : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean menuButtonAction;

    public GameObject menu;

    private GameObject leftHand;
    private GameObject rightHand;
    private SteamVR_Input_Sources currentHandParent;
    private GameObject handPrefabL;
    private GameObject handPrefabR;

    public RectTransform myPanel;
    public GameObject menuTextPrefab;

    private void Awake()
    {
        GameObject cameraRig = GameObject.FindGameObjectWithTag("MainCamera");
        leftHand = cameraRig.transform.Find("LeftHand").gameObject;
        rightHand = cameraRig.transform.Find("RightHand").gameObject;

        handPrefabL = transform.Find("HandPrefabL").gameObject;
        handPrefabR = transform.Find("HandPrefabR").gameObject;
    }

    private void ToggleMenu(SteamVR_Input_Sources hand)
    {
        // Only deactivate the menu if the same button is pressed twice, swap hand if not
        if (menu.activeInHierarchy && hand.Equals(currentHandParent))
        {
            SetEnablelaserOnHand(handPrefabR, false);
            SetEnablelaserOnHand(handPrefabL, false);
            menu.SetActive(false);
        }
        else
        {
            if (hand == SteamVR_Input_Sources.LeftHand)
            {
                menu.transform.SetParent(leftHand.transform);
                currentHandParent = SteamVR_Input_Sources.LeftHand;
                SetEnablelaserOnHand(handPrefabR, true);
                SetEnablelaserOnHand(handPrefabL, false);
            }
            else if (hand == SteamVR_Input_Sources.RightHand)
            {
                menu.transform.SetParent(rightHand.transform);
                currentHandParent = SteamVR_Input_Sources.RightHand;
                SetEnablelaserOnHand(handPrefabL, true);
                SetEnablelaserOnHand(handPrefabR, false);
            }
            else
            {
                Debug.LogError("Must be attached to either left hand or right hand");
            }

            menu.transform.localPosition = new Vector3(0, -0.25f, 0.25f);
            menu.transform.localRotation = Quaternion.Euler(60, 0, 0);
            menu.SetActive(true);
        }
    }

    private void SetEnablelaserOnHand(GameObject hand, bool enabled)
    {
        LineRenderer lr = hand.GetComponent<LineRenderer>();
        MenuLaser menuLaser = hand.GetComponent<MenuLaser>();
        lr.enabled = enabled;
        menuLaser.enabled = enabled;
    }

    public void AddTextBlock(string text)
    {
        GameObject newText = (GameObject)Instantiate(menuTextPrefab, myPanel.transform);
        newText.GetComponent<Text>().text = text;
    }

    private void Update()
    {
        if (menuButtonAction.GetStateDown(handType))
        {
            if (menuButtonAction.GetStateDown(SteamVR_Input_Sources.LeftHand))
            {
                ToggleMenu(SteamVR_Input_Sources.LeftHand);
            }
            else if (menuButtonAction.GetStateDown(SteamVR_Input_Sources.RightHand)) {
                ToggleMenu(SteamVR_Input_Sources.RightHand);
            }
        }
    }
}
