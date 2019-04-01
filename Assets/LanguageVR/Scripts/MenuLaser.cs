using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.UI;
using NTNU.CarloMarton.VRLanguage;
using Photon.Pun;
using System;

namespace NTNU.CarloMarton.VRLanguage
{
	public class MenuLaser : MonoBehaviour
	{
		private bool pinchPressed = false;
		private Color standardLaserColor;
		private Color deleteModeLaserColor;
		private Color editModeLaserColor;
		private Color standByColor = Color.black;
		private Color hoverColor = Color.white;
		private Color clickedColor = Color.red;
		private GameObject clickedButton;
		private GameObject clickedObject;
		private LineRenderer lr;
		private Ray laser;
		private bool deleteToggle;
		private bool editToggle;


		private float timer;
		private float timerThreshold = 1.0f; // How many seconds must the delete button be held to delete object
		private bool clickBeingHeld = false;
   
		public SteamVR_Input_Sources handType;
		public SteamVR_Action_Boolean laserClickButton;
		public SteamVR_Action_Boolean touchInput;
		public SteamVR_Action_Vector2 touchDirectionInput;

		public GameObject menu;
		public GameObject mainPanel;

		private GameObject dictionaryPanel;
		private RectTransform dictionaryScrollContent;
		private ScrollRect dictionaryScrollRect;

		private GameObject loadStatePanel;
		private RectTransform loadStateScrollContent;
		private ScrollRect loadStateScrollRect;


		public GameObject HelperUI;
		private GameObject deleteProgressPanel;
		private Slider deleteProgressSlider;
		private TextMesh helperUIText;

		private bool isClickingButton = false;

		// Start is called before the first frame update
		void Start()
		{
			deleteProgressPanel = HelperUI.transform.Find("CanvasUI/DeleteProgressBar").gameObject;
			dictionaryPanel = mainPanel.transform.Find("DictionaryPanel").gameObject;
			loadStatePanel = mainPanel.transform.Find("LoadStatePanel").gameObject;

			deleteProgressSlider = deleteProgressPanel.transform.Find("DeleteProgressSlider").GetComponent<Slider>();
			deleteProgressSlider.maxValue = timerThreshold;
			helperUIText = HelperUI.GetComponent<TextMesh>();

			standardLaserColor = new Color(0.46f, 0.98f, 0.56f);
			deleteModeLaserColor = new Color(0.878f, 0.277f, 0.345f);
			editModeLaserColor = Color.blue;
			deleteToggle = false;
			editToggle = false;
			lr = GetComponent<LineRenderer>();
			laser = new Ray();

			timer = 0;
       
			SetLaserColor(standardLaserColor);

			dictionaryScrollContent = dictionaryPanel.transform.Find("ScrollContent").GetComponent<RectTransform>();
			dictionaryScrollRect = dictionaryPanel.transform.GetComponent<ScrollRect>();
			loadStateScrollContent = loadStatePanel.transform.Find("ScrollContent").GetComponent<RectTransform>();
			loadStateScrollRect = loadStatePanel.transform.GetComponent<ScrollRect>();
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

		public void ToggleDeleteMode(bool toggleMode)
		{
			deleteToggle = toggleMode;
			editToggle = false;
		}

		public void ToggleEditMode(bool toggleMode)
		{
			editToggle = toggleMode;
			deleteToggle = false;
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
			else if (editToggle)
			{
				SetLaserColor(editModeLaserColor);
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
				else if (deleteToggle && Physics.Raycast(laser, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("DeletableObjects"), QueryTriggerInteraction.Ignore))
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

				// Editing objects
				else if (editToggle && Physics.Raycast(laser, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("DeletableObjects"), QueryTriggerInteraction.Ignore))
				{
					clickedObject = raycastHit.collider.gameObject;
					Debug.Log(clickedObject.transform.parent);
					if (clickedObject.transform.parent != null && clickedObject.transform.parent.gameObject.layer == 10)
					{
						clickedObject = clickedObject.transform.parent.gameObject;
					}

					clickedObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
					EditObject(clickedObject);
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
						helperUIText.text = ButtonNameToButtonDescription(clickedButton.name);
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
				if (dictionaryScrollContent.gameObject.activeInHierarchy)
					ScrollScrollContent(dictionaryScrollContent);
				if (loadStateScrollContent.gameObject.activeInHierarchy)
					ScrollScrollContent(loadStateScrollContent);
			}

		}

		private void EditObject(GameObject clickedObject) {
			EditGameObject ego = clickedObject.GetComponent<EditGameObject>();
			if (ego == null)
				return;

			ego.NextVariation();
			clickedObject.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
		}

		private void ScrollScrollContent(RectTransform scrollContent)
		{
			float currentPosY = scrollContent.GetComponent<RectTransform>().localPosition.y;
			float threshold = 0f;
			float yDirection = GetTouchDirection().y;
			float moveSpeed = 20f;

			float contentHeight = CalculateHeightOfContent(scrollContent);
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

		private string ButtonNameToButtonDescription(string buttonName)
		{
			switch (buttonName)
			{
				case "PlayAudioButton":
					return "Spill av lyd";
				case "AddInteractableObjectButton":
					string interactableObjectName = clickedButton.transform.parent.GetComponentInChildren<Text>().text;
					return "Legg til " + interactableObjectName;
				case "InvisibilityButton":
					return "Bli usynlig";
				case "DeleteObjectButton":
					return "Skru av/på slettemodus";
				case "EditObjectButton":
					return "Skru av/på endremodus";
				case "SettingsButton":
					return "Innstillinger";
				case "ExitGameButton":
					return "Avslutt";
				case "SaveStateButton":
					return "Lagre verden";
				case "LoadStateButton":
					return "Last inn verden";
				case "DeleteStateButton":
					return "Slett lagret verden";
				case "ToggleTextButton":
					return "Skru av/på objekttekst";
				case "ChangeColourButton":
					return "Endre farge på avatar";
                case "ActivateVoiceRecognitionButton":
                    return "Start stemmegjennkjenning";
				case "Navy":
				case "Blue":
				case "Aqua":
				case "Teal":
				case "Olive":
				case "Green":
				case "Lime":
				case "Yellow":
				case "Orange":
				case "Red":
				case "Chocolate":
				case "Maroon":
				case "Fuchsia":
				case "Purple":
				case "Black":
				case "Grey":
				case "Silver":
				case "White":
					return "Endre farge til " + TranslateColorToNorwegian(buttonName);
				case "CancelButton":
					return "Avbryt";
                case "Backbutton":
                    return "Tilbake";

				default:
					return "Ikke lagt til hint på knapp";
			}
		}

		private string TranslateColorToNorwegian(string englishColor)
		{
			switch (englishColor)
			{
				case "Navy":
					return "marineblå";
				case "Blue":
					return "blå";
				case "Aqua":
					return "cyan";
				case "Teal":
					return "grønnblå";
				case "Olive":
					return "olivengrønn";
				case "Green":
					return "grønn";
				case "Lime":
					return "limegrønn";
				case "Yellow":
					return "gul";
				case "Orange":
					return "oransje";
				case "Red":
					return "rød";
				case "Chocolate":
					return "sjokoladebrunt";
				case "Maroon":
					return "rødbrun";
				case "Fuchsia":
					return "magentarød";
				case "Purple":
					return "lilla";
				case "Black":
					return "svart";
				case "Grey":
					return "grå";
				case "Silver":
					return "sølv";
				case "White":
					return "hvit";
				default:
					return "ukjent";
			}
		}

		private float CalculateHeightOfContent(RectTransform scrollContent)
		{
			float childHeight = scrollContent.GetChild(0).GetComponent<RectTransform>().rect.height;
            float spacing = scrollContent.GetComponent<VerticalLayoutGroup>().spacing;
            float paddingBottom = scrollContent.GetComponent<VerticalLayoutGroup>().padding.bottom;
			float paddingTop = scrollContent.GetComponent<VerticalLayoutGroup>().padding.top;

			return scrollContent.childCount * (childHeight + spacing) + paddingBottom + paddingTop;
		}

		private void SetButtonColor(Button button, Color color)
		{
			ColorBlock colorBlock = new ColorBlock();
			colorBlock.normalColor = color;
			colorBlock.colorMultiplier = 1;
			button.colors = colorBlock;
		}

		public GameObject IsClickingButton()
		{
			if (isClickingButton)
			{
				return clickedButton;
			}
			else
			{
				return null;
			}
		}
	}
}
