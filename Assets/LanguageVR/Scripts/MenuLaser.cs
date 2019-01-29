﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.UI;

public class MenuLaser : MonoBehaviour
{
    private bool pinchPressed = false;
    private Color standByColor = Color.black;
    private Color hoverColor = Color.white;
    private Color clickedColor = Color.red;
    private GameObject clickedObject;
    private ColorBlock buttonColor;
    private LineRenderer lr;
    private Ray laser;
   

    // Start is called before the first frame update
    void Start()
    {        
        lr = GetComponent<LineRenderer>();
        laser = new Ray();

        lr.startColor = new Color(0.70588f, 1f, 0.56863f);
        lr.endColor = new Color(0.70588f, 1f, 0.56863f);

    }

    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean laserClickButton;

    public bool GetLaserButtonClicked()
    {
        return laserClickButton.GetLastStateDown(handType);
    }

    void toggleLaser(bool toggleMode)
    {
        lr.enabled = toggleMode;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit raycastHit;

        laser.origin = this.transform.position;
        laser.direction = this.transform.forward;

        //When button is clicked
        if (GetLaserButtonClicked())
        {
            
            if (Physics.Raycast(laser, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("InWorldUI"), QueryTriggerInteraction.Ignore))
            {
                clickedObject = raycastHit.collider.gameObject;
                if (clickedObject.GetComponent<Button>())
                {
                    print("Ok");
                    pinchPressed = true;
                    SetButtonColor(clickedObject.GetComponent<Button>(), standByColor);
                    toggleLaser(false);
                    print(clickedObject.GetComponent<Text>().text);
                }
            }
        }

        //When laser goes away from a button
        if (!GetLaserButtonClicked())
        {
            pinchPressed = false;
            if (clickedObject != null && clickedObject.GetComponent<Button>())
            {
                SetButtonColor(clickedObject.GetComponent<Button>(), standByColor);
                clickedObject = null;
            }

            toggleLaser(true);
        }


        if (!pinchPressed)
        {
            //When laser hovers over button

            lr.enabled = true;
            lr.SetPosition(0, transform.position);

            if (Physics.Raycast(laser, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("InWorldUI"), QueryTriggerInteraction.Ignore))
            {
                if (clickedObject != null && raycastHit.collider.gameObject != clickedObject)
                {
                    SetButtonColor(clickedObject.GetComponent<Button>(), standByColor);
                    raycastHit.collider.gameObject.GetComponent<Button>().colors = buttonColor;
                }
                lr.SetPosition(1, raycastHit.point);

                clickedObject = raycastHit.collider.gameObject;
                if (clickedObject.GetComponent<Button>())
                {
                    buttonColor = clickedObject.GetComponent<Button>().colors;
                    SetButtonColor(clickedObject.GetComponent<Button>(), hoverColor);
                    clickedObject.GetComponent<Button>().colors = buttonColor;
                }

            }
            else
            {
                lr.SetPosition(1, laser.origin + laser.direction * 3f);
                if (clickedObject != null && clickedObject.GetComponent<Button>())
                {
                    SetButtonColor(clickedObject.GetComponent<Button>(), standByColor);
                    clickedObject.GetComponent<Button>().colors = buttonColor;
                }
            }

        }
    }
    private void SetButtonColor(Button button, Color color)
    {
        buttonColor.normalColor = color;
        buttonColor.colorMultiplier = 5;
        button.colors = buttonColor;
    }
}
