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

    public RectTransform myPanel;
    public GameObject menuTextPrefab;

    List<string> chatEvents;
    private float nextMessage;
    private int myNumber = 0;

    private void Awake()
    {
        GameObject cameraRig = GameObject.FindGameObjectWithTag("MainCamera");
        leftHand = cameraRig.transform.Find("LeftHand").gameObject;
        rightHand = cameraRig.transform.Find("RightHand").gameObject;

        Foo();
    }

    private void ToggleMenu(SteamVR_Input_Sources hand)
    {
        // Only deactivate the menu if the same button is pressed twice, swap hand if not
        if (menu.activeInHierarchy && hand.Equals(currentHandParent))
        {
            menu.SetActive(false);
        }
        else
        {
            if (hand == SteamVR_Input_Sources.LeftHand)
            {
                menu.transform.SetParent(leftHand.transform);
                currentHandParent = SteamVR_Input_Sources.LeftHand;
            }
            else if (hand == SteamVR_Input_Sources.RightHand)
            {
                menu.transform.SetParent(rightHand.transform);
                currentHandParent = SteamVR_Input_Sources.RightHand;
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

    private void Foo()
    {
        chatEvents = new List<string>();
        chatEvents.Add("this");
        chatEvents.Add("is");
        chatEvents.Add("a");
        chatEvents.Add("test");
        chatEvents.Add("for");
        chatEvents.Add("showing");
        chatEvents.Add("strings");
        chatEvents.Add("and");
        chatEvents.Add("displaying");
        chatEvents.Add("in");
        chatEvents.Add("a");
        chatEvents.Add("scrollable");
        chatEvents.Add("panel");
        chatEvents.Add("in");
        chatEvents.Add("the");
        chatEvents.Add("new");
        chatEvents.Add("GUI");
        chatEvents.Add("really we can put anything in this");
        chatEvents.Add("anything at all");
        chatEvents.Add("Knock, knock");
        chatEvents.Add("who's there");
        chatEvents.Add("Doctor");
        chatEvents.Add("Doctor who?");
        chatEvents.Add("I refuse to participate in a joke older than I am");
        chatEvents.Add("yeah right older than you are!");
        chatEvents.Add("It is older than me honest");
        chatEvents.Add("as if, remember I know how old you are");
        chatEvents.Add("Look there's clear evidence that they started in 1930's");
        chatEvents.Add("Maybe but Dr Who didn't did it");
        chatEvents.Add("Well OK but Dr Who's older than me");
        chatEvents.Add("Yes but that joke didn't start with the first episode");
        chatEvents.Add("Right I've had enough of this");
        chatEvents.Add("Stop being so melodramatic");
        nextMessage = Time.time + 1f;
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

        //if (Time.time > nextMessage && myNumber < chatEvents.Count)
        //{
        //    AddInteractableObject(chatEvents[myNumber]);
        //    myNumber++;
        //    nextMessage = Time.time + 1f;
        //}
    }
}
