using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.UI;

public class MenuLaser : MonoBehaviour
{
    private bool arrowPress = false;
    private Color standByColor = Color.white;
    private Color hoverColor = Color.yellow;
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
        lr.startColor = Color.green;
        lr.endColor = Color.green;
    }

    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean changeAction;

    public bool getArrowDown()
    {
        return changeAction.GetLastStateDown(handType);
    }

    public bool getArrowUp()
    {
        return changeAction.GetLastStateUp(handType);
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
        if (getArrowDown())
        {
            
            if (Physics.Raycast(laser, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("UI"), QueryTriggerInteraction.Ignore))
            {
                print("Ok");
                arrowPress = true;
                clickedObject = raycastHit.collider.gameObject;
                buttonColor = raycastHit.collider.gameObject.GetComponent<Button>().colors;
                buttonColor.normalColor = clickedColor;
                raycastHit.collider.gameObject.GetComponent<Button>().colors = buttonColor;
                toggleLaser(false);
            }
        }

        //When laser goes away from a button
        if (!getArrowDown())
        {
            arrowPress = false;
            if (clickedObject != null)
            {
                buttonColor.normalColor = standByColor;
                clickedObject.gameObject.GetComponent<Button>().colors = buttonColor;
                clickedObject = null;
            }

            toggleLaser(true);
        }


        if (!arrowPress)
        {
            //When laser hovers over button

            lr.enabled = true;
            lr.SetPosition(0, transform.position);

            if (Physics.Raycast(laser, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("UI"), QueryTriggerInteraction.Ignore))
            {
                if (clickedObject != null && raycastHit.collider.gameObject != clickedObject)
                {
                    buttonColor.normalColor = standByColor;
                    raycastHit.collider.gameObject.GetComponent<Button>().colors = buttonColor;
                }
                clickedObject = raycastHit.collider.gameObject;
                buttonColor = raycastHit.collider.gameObject.GetComponent<Button>().colors;
                buttonColor.normalColor = hoverColor;
                raycastHit.collider.gameObject.GetComponent<Button>().colors = buttonColor;
                lr.SetPosition(1, raycastHit.point);

            }
            else
            {
                lr.SetPosition(1, laser.origin + laser.direction * 3f);
                if (clickedObject != null)
                {
                    buttonColor.normalColor = standByColor;
                    clickedObject.gameObject.GetComponent<Button>().colors = buttonColor;
                }
            }

        }

       
    }
}
