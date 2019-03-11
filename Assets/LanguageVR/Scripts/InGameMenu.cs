﻿using ExitGames.Client.Photon;
using NTNU.CarloMarton.VRLanguage;
using Photon.Pun;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class InGameMenu : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean menuButtonAction;

    public GameObject menu;
    public GameObject helperUI;

    private GameObject leftHand;
    private GameObject rightHand;
    private SteamVR_Input_Sources currentHandParent;
    private GameObject laserHand;
    private GameObject handPrefabL;
    private GameObject handPrefabR;

    public RectTransform myPanel;
    public GameObject menuTextPrefab;

    private void Awake()
    {
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
    }

    private void ToggleMenu(SteamVR_Input_Sources hand)
    {
        // Don't do anything if it is not your own menu
        if (!gameObject.GetComponent<PhotonView>().IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

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
            menu.transform.Find("TopPanel").transform.Find("DeleteObjectButton").GetComponent<InGameMenuUI>().resetDeleteObjectMode();
            menu.SetActive(true);

            helperUI.transform.localPosition = new Vector3(0, 0, 0.01f);
            helperUI.transform.localRotation = Quaternion.Euler(60, 0, 0);
            helperUI.SetActive(true);
        }
    }

    private void SetEnablelaserOnHand(GameObject hand, bool enabled)
    {
        if (gameObject.GetComponent<PhotonView>().IsMine || !PhotonNetwork.IsConnected)
        {
            LineRenderer lr = hand.GetComponent<LineRenderer>();
            MenuLaser menuLaser = hand.GetComponent<MenuLaser>();
            menuLaser.toggleDeleteMode(false);
            lr.enabled = enabled;
            menuLaser.enabled = enabled;
        }
    }

    public GameObject GetLaserHand()
    {
        return laserHand;
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
