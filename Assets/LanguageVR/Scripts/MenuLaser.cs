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
   
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean laserClickButton;
    public SteamVR_Action_Boolean touchInput;
    public SteamVR_Action_Vector2 touchDirectionInput;

    public GameObject menu;
    private RectTransform scrollContent;
    private ScrollRect scrollRect;

    // Start is called before the first frame update
    void Start()
    {        
        lr = GetComponent<LineRenderer>();
        laser = new Ray();

        lr.startColor = new Color(0.70588f, 1f, 0.56863f);
        lr.endColor = new Color(0.70588f, 1f, 0.56863f);

        scrollContent = menu.transform.Find("MainPanel").Find("ScrollRect").Find("ScrollContent").GetComponent<RectTransform>();
        scrollRect = menu.transform.Find("MainPanel").Find("ScrollRect").GetComponent<ScrollRect>();
    }

    public bool GetLaserButtonClicked()
    {
        return laserClickButton.GetLastStateDown(handType);
    }

    public bool GetTrackpadTouched()
    {
        return touchInput.GetState(handType);
    }

    public Vector2 GetTouchDirection()
    {
        return touchDirectionInput.GetLastAxis(handType);
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
                if (clickedObject.GetComponent<Button>() && clickedObject.GetComponent<InGameMenuUI>())
                {
                    pinchPressed = true;
                    SetButtonColor(clickedObject.GetComponent<Button>(), clickedObject.GetComponent<InGameMenuUI>().GetNormalColor());
                    toggleLaser(false);

                    clickedObject.GetComponent<Button>().onClick.Invoke();
                }
            }
        }

        //When laser goes away from a button
        if (!GetLaserButtonClicked())
        {
            pinchPressed = false;
            if (clickedObject != null && clickedObject.GetComponent<Button>() && clickedObject.GetComponent<InGameMenuUI>())
            {
                SetButtonColor(clickedObject.GetComponent<Button>(), clickedObject.GetComponent<InGameMenuUI>().GetNormalColor());
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
                if (clickedObject != null && clickedObject.GetComponent<Button>() && clickedObject.GetComponent<InGameMenuUI>() && raycastHit.collider.gameObject != clickedObject)
                {
                    SetButtonColor(clickedObject.GetComponent<Button>(), clickedObject.GetComponent<InGameMenuUI>().GetPressedColor());
                }
                lr.SetPosition(1, raycastHit.point);

                clickedObject = raycastHit.collider.gameObject;
                if (clickedObject.GetComponent<Button>())
                {
                    SetButtonColor(clickedObject.GetComponent<Button>(), clickedObject.GetComponent<InGameMenuUI>().GetHighlightedColor());
                }

            }
            else
            {
                lr.SetPosition(1, laser.origin + laser.direction * 3f);
                if (clickedObject != null && clickedObject.GetComponent<Button>() && clickedObject.GetComponent<InGameMenuUI>())
                {
                    SetButtonColor(clickedObject.GetComponent<Button>(), clickedObject.GetComponent<InGameMenuUI>().GetNormalColor());
                }
            }

        }

        // Scrolling in the menu
        if (GetTrackpadTouched())
        {
            float currentPosY = scrollContent.GetComponent<RectTransform>().localPosition.y;
            float threshold = 0f;
            float yDirection = GetTouchDirection().y;
            float moveSpeed = 5f;

            float contentHeight = CalculateHeightOfContent();
            float mainPanelHeight = scrollContent.GetComponent<RectTransform>().rect.height;

            if (yDirection < -threshold && currentPosY + mainPanelHeight < contentHeight) // Down
            {
                scrollContent.transform.localPosition -= new Vector3(0f, moveSpeed * yDirection, 0f);
            }
            else if (yDirection > threshold && currentPosY > 0) // Up
            {
                scrollContent.transform.localPosition -= new Vector3(0f, moveSpeed * yDirection, 0f);
            }
        }

    }

    private float CalculateHeightOfContent()
    {
        float childHeight = scrollContent.GetChild(0).GetComponent<RectTransform>().rect.height;
        float padding = scrollContent.GetComponent<VerticalLayoutGroup>().padding.bottom;
        return scrollContent.childCount * (childHeight);
    }

    private void SetButtonColor(Button button, Color color)
    {
        //button.image.color = color;
        buttonColor.normalColor = color;
        buttonColor.colorMultiplier = 5;
        button.colors = buttonColor;
    }
}
