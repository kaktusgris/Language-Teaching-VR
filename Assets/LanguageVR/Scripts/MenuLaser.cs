using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.UI;
using NTNU.CarloMarton.VRLanguage;
using Photon.Pun;

public class MenuLaser : MonoBehaviour
{
    private bool pinchPressed = false;
    private Color standardLaserColor;
    private Color deleteModeLaserColor;
    private Color standByColor = Color.black;
    private Color hoverColor = Color.white;
    private Color clickedColor = Color.red;
    private GameObject clickedButton;
    private GameObject clickedObject;
    private ColorBlock buttonColor;
    private LineRenderer lr;
    private Ray laser;
    private bool deleteToggle;


    private float timer;
    private float timerThreshold = 3.0f;
    private bool clickBeingHeld = false;
   
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean laserClickButton;
    public SteamVR_Action_Boolean touchInput;
    public SteamVR_Action_Vector2 touchDirectionInput;

    public GameObject menu;
    private RectTransform scrollContent;
    private ScrollRect scrollRect;
    public GameObject mainPanel;
    private GameObject scrollRectPanel;

    public GameObject HelperUI;
    private GameObject deleteProgressPanel;
    private Slider deleteProgressSlider;
    private TextMesh helperUIText;

    private bool isClickingButton = false;

    // Start is called before the first frame update
    void Start()
    {
        deleteProgressPanel = HelperUI.transform.Find("CanvasUI/DeleteProgressBar").gameObject;
        scrollRectPanel = mainPanel.transform.Find("ScrollRect").gameObject;
        deleteProgressSlider = deleteProgressPanel.transform.Find("DeleteProgressSlider").GetComponent<Slider>();
        deleteProgressSlider.maxValue = timerThreshold;
        helperUIText = HelperUI.GetComponent<TextMesh>();

        standardLaserColor = new Color(0.46f, 0.98f, 0.56f);
        deleteModeLaserColor = new Color(0.878f, 0.277f, 0.345f);
        deleteToggle = false;
        lr = GetComponent<LineRenderer>();
        laser = new Ray();

        timer = 0;
       
        SetLaserColor(standardLaserColor);

        scrollContent = menu.transform.Find("MainPanel").Find("ScrollRect").Find("ScrollContent").GetComponent<RectTransform>();
        scrollRect = menu.transform.Find("MainPanel").Find("ScrollRect").GetComponent<ScrollRect>();
    }

    

    public void SetLaserColor(Color laserColor)
    {
        lr.startColor = laserColor;
        lr.endColor = laserColor;
    }

    public bool GetLaserButtonClicked()
    {
        return laserClickButton.GetLastStateDown(handType);
    }


    public bool GetLaserButtonNotClicked()
    {
        return laserClickButton.GetLastStateUp(handType);
    }

    public void toggleDeleteMode(bool toggleMode)
    {
        deleteToggle = toggleMode;

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

        if (deleteToggle)
        {
            SetLaserColor(deleteModeLaserColor);
        }
        else
        {
            SetLaserColor(standardLaserColor);
        }
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit raycastHit;
        isClickingButton = false;

        laser.origin = this.transform.position;
        laser.direction = this.transform.forward;

        //When button is clicked
        if (GetLaserButtonClicked())
        {

            if (Physics.Raycast(laser, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("InWorldUI"), QueryTriggerInteraction.Ignore))
            {
                clickedButton = raycastHit.collider.gameObject;
                if (clickedButton.GetComponent<Button>() && clickedButton.GetComponent<InGameMenuUI>())
                {
                    pinchPressed = true;
                    SetButtonColor(clickedButton.GetComponent<Button>(), clickedButton.GetComponent<InGameMenuUI>().GetNormalColor());
                    toggleLaser(false);

                    clickedButton.GetComponent<Button>().onClick.Invoke();
                    isClickingButton = true;
                }
            }
            // Deleting objects
            else if (Physics.Raycast(laser, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("DeletableObjects"), QueryTriggerInteraction.Ignore) && deleteToggle)
            {
                clickedObject = raycastHit.collider.gameObject;
                Debug.Log(clickedObject.transform.parent);
                if (clickedObject.transform.parent != null && clickedObject.transform.parent.gameObject.layer == 10)
                {
                    clickedObject = clickedObject.transform.parent.gameObject;
                }

                clickedObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                clickBeingHeld = true;
                deleteProgressPanel.SetActive(true);
                
                deleteProgressPanel.transform.Find("DeleteProgressText").GetComponent<Text>().text = "Sletter \"" + clickedObject.transform.Find("Text").GetComponent<TextMesh>().text + "\"...";

                toggleLaser(false);
            }
        }

        if (GetLaserButtonNotClicked())
        {
            clickBeingHeld = false;
            deleteProgressPanel.SetActive(false);
            timer = 0;
            deleteProgressSlider.value = timer;
        } else if (clickBeingHeld) {
            toggleLaser(false);
            timer += Time.deltaTime;
            deleteProgressSlider.value = timer;
            
            if (timer > timerThreshold)
            {
                PhotonNetwork.Destroy(clickedObject);
                clickBeingHeld = false;
                deleteProgressPanel.SetActive(false);
                timer = 0;
                deleteProgressSlider.value = timer;
            }
        }


        //When laser goes away from a button
        if (!GetLaserButtonClicked() && !clickBeingHeld)
        {
            pinchPressed = false;
            if (clickedButton != null && clickedButton.GetComponent<Button>() && clickedButton.GetComponent<InGameMenuUI>())
            {
                SetButtonColor(clickedButton.GetComponent<Button>(), clickedButton.GetComponent<InGameMenuUI>().GetNormalColor());
                clickedButton = null;
                helperUIText.text = "";
            } else if (clickedObject != null)
            {
                clickedButton = null;
            }

            toggleLaser(true);
        }


        if (!pinchPressed && !clickBeingHeld)
        {
            //When laser hovers over button
            toggleLaser(true);
            lr.SetPosition(0, transform.position);

            if (Physics.Raycast(laser, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("InWorldUI"), QueryTriggerInteraction.Ignore))
            {
                if (clickedButton != null && clickedButton.GetComponent<Button>() && clickedButton.GetComponent<InGameMenuUI>() && raycastHit.collider.gameObject != clickedButton)
                {
                    SetButtonColor(clickedButton.GetComponent<Button>(), clickedButton.GetComponent<InGameMenuUI>().GetPressedColor());
                }
                lr.SetPosition(1, raycastHit.point);

                clickedButton = raycastHit.collider.gameObject;
                if (clickedButton.GetComponent<Button>())
                {
                    SetButtonColor(clickedButton.GetComponent<Button>(), clickedButton.GetComponent<InGameMenuUI>().GetHighlightedColor());
                    helperUIText.text = clickedButton.name;
                    print(clickedButton.name);
                }

            } else if (Physics.Raycast(laser, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("DeletableObjects"), QueryTriggerInteraction.Ignore)) {
                    lr.SetPosition(1, raycastHit.point);
                    clickedObject = raycastHit.collider.gameObject;
            }
            else
            {
                lr.SetPosition(1, laser.origin + laser.direction * 3f);
                if (clickedButton != null && clickedButton.GetComponent<Button>() && clickedButton.GetComponent<InGameMenuUI>())
                {
                    SetButtonColor(clickedButton.GetComponent<Button>(), clickedButton.GetComponent<InGameMenuUI>().GetNormalColor());
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

    public bool IsClinkingButton()
    {
        return isClickingButton;
    }
}
