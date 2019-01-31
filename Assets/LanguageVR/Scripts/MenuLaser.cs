using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.UI;
using NTNU.CarloMarton.VRLanguage;

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
                    pinchPressed = true;
                    SetButtonColor(clickedObject.GetComponent<Button>(), standByColor);
                    toggleLaser(false);

                    ObjectClicked(clickedObject);
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
                if (clickedObject != null && clickedObject.GetComponent<Button>() && raycastHit.collider.gameObject != clickedObject)
                {
                    SetButtonColor(clickedObject.GetComponent<Button>(), standByColor);
                }
                lr.SetPosition(1, raycastHit.point);

                clickedObject = raycastHit.collider.gameObject;
                if (clickedObject.GetComponent<Button>())
                {
                    SetButtonColor(clickedObject.GetComponent<Button>(), hoverColor);
                }

            }
            else
            {
                lr.SetPosition(1, laser.origin + laser.direction * 3f);
                if (clickedObject != null && clickedObject.GetComponent<Button>())
                {
                    SetButtonColor(clickedObject.GetComponent<Button>(), standByColor);
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

    private void ObjectClicked(GameObject go)
    {
        string objectName = go.GetComponent<Text>().text;
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
