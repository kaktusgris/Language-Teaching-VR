using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace NTNU.CarloMarton.VRLanguage
{
    public class Tutorial : MonoBehaviour
    {
        [SerializeField] private List<Text> instructionTexts;
        [SerializeField] private GameObject objectToInteractWith;
        [SerializeField] private List<GameObject> objectsToSpawn;

        [Header ("Actions")]
        [SerializeField] private SteamVR_Action_Boolean grabPinchAction;
        [SerializeField] private SteamVR_Action_Boolean menuAction;
        [SerializeField] private SteamVR_Action_Boolean teleportAction;
        [SerializeField] private SteamVR_Action_Boolean interactUIAction;
        [SerializeField] private SteamVR_Action_Boolean touchpadTouchedAction;

        [Header("Hand hints")]
        [SerializeField] private string teleportHoldHint = "Hold for å teleportere / Hold to teleport";
        [SerializeField] private string teleportLetGoHint = "Slipp for å teleportere / Let go to teleport";
        [SerializeField] private string moveToGrabHint = "Flytt hånda til kameraet / Move the hand to the camera";
        [SerializeField] private string grabHint = "Plukk opp kameraet / Pick up the camera";
        [SerializeField] private string letGoHint = "Slipp eller kast / Let go or throw";
        [SerializeField] private string openMenuHint = "Åpne meny / Open menu";
        [SerializeField] private string closeMenuHint = "Lukk meny / Close menu";
        [SerializeField] private string interactWithMenuHint = "Klikk på knapper / Click buttons";
        [SerializeField] private string swapMenuHint = "Bytt menyhånd / Swap menu hand";
        [SerializeField] private string scrollHint = "Bla / Scroll";
        [SerializeField] private string deleteHint = "Slett / Delete";

        [Header("Canvas instructions")]
        [SerializeField] private string teleportInstructions = "Teleporter rundt.\n -------- \nTeleport around the space.";
        [SerializeField] private string pickUpInstructions = "Plukk opp kameraet og kast det.\n -------- \nPick up the camera and throw it.";
        [SerializeField] private string openMenuInstructions = "Åpne menyen og se her for videre instruksjoner.\n -------- \nOpen the menu and look here for further instructions.";
        [SerializeField] private string playAudioInstructions = "Spill av et lydklipp ved å trykke på lydikonet.\n -------- \nPlay an audio clip by pressing the sound icon.";
        [SerializeField] private string voiceRecInstructions = "Legg til et objekt ved bruk av stemmen ved å trykke på snakkebobleknappen. Hvis du nå sier ordet innen 15 sekund vil objektet dukke opp.\n -------- \nSpawn an object with voice commands by pressing the speach bubble button. If you then say the word within 15 seconds the object will appear.";

        [SerializeField] private string spawnObjectInstructions = "Legg til et nytt objekt i verden ved å trykke på plussikonet.\nEt nytt objekt av typen du trykker på vil da dukke opp i verden.";
        [SerializeField] private string deleteLaserInstructions = "Slett kameraet ved å trykke på søppelkasseikonet. Dette vil endre fargen på laseren til rød og kan nå slette ting.";
        [SerializeField] private string deleteInstructions = "Fullfør sletting av kamera ved å peke laseren på kameraet for så å holde nede klikkeknappen i et sekund.\nDu vil da se et vindu komme opp og etter de to sekundene vil kameraet forsvinne.\nAlle objekter som kan interageres med kan slettes.";
        [SerializeField] private string editLaserInstructions = "Endre utseende til objekter ved å trykke på blyandikonet. Dette vil endre fargen på laseren til blå og du kan nå endre på objekter.";
        [SerializeField] private string editInstructions = "Fullfør endring av objekter ved å peke laseren på dem og klikke.\nAlle objekter som kan interageres med kan endres: farge, størrelse eller begge deler.\n\nKlikk på instillingsknappen i venstre hjørne for å fortsette.";
        [SerializeField] private string invisibleInstructions = "Bli usynlig ved å trykke på øyeikonet.\nSå lenge du er usynlig vil ingen andre kunne se deg, men du kan fortsatt interagere med objekter og observere.\nTrykk på knappen igjen for å bli synlig igjen.";
        [SerializeField] private string saveInstructions = "Lagre rommet ved å klikke på lagre rom knappen i instillingervinduet.\nLagra rom er tilgjengelig på maskinen de ble lagret på så lenge de ikke blir slettet.";
        [SerializeField] private string beginLoadInstructions = "Last inn rommet som ble lagra ved å klikke på last rom knappen i instillingervinduet.\nAlle lagrede rom er tilgjengelig i tillegg til et tomt rom og standardrommet og kan lastes inn når som helst.";
        [SerializeField] private string loadInstructions = "Trykk på nedlastingsknappen for å laste inn rommet. Du kan også trykke på sletteknappen for å slette det lagrede rommet.";
        [SerializeField] private string toggleObjectTextInstructions = "Trykk på tekstknappen i instillingsvinduet for å skru av og på tekst på objekter.\nPlukk opp et objekt før og etter du har trykket for å se forskjell.";
        [SerializeField] private string exitGameInstructions = "Avslutt spillet ved å trykke på krysset.\n -------- \nExit the game by pressing the cross.";

        private Valve.VR.InteractionSystem.Player player;
        private int interactableObjectsInSceneAtStart;

        private int tutorialStep = 0;

        void Start()
        {
            player = Valve.VR.InteractionSystem.Player.instance;
            EnableAdditionalObjects(false);
            StartCoroutine(HintManagerCoroutine());
            interactableObjectsInSceneAtStart = GameObject.FindGameObjectsWithTag("InteractableObject").Length;
            SetDefaultStateIfNotSetAlready();
        }

        

        private IEnumerator HintManagerCoroutine()
        {
            int menuStep = 4;

            int interactWithMenuCheckpoint = menuStep + 1;

            Hand handWithLaser = GetHandWithActiveLaser();
            Hand previousHandWithLaser = null;
            MenuLaser menuLaser = null;

            while (tutorialStep != -1)
            {
                switch (tutorialStep)
                {
                    #region Teleport start
                    case 0:
                        ShowButtonHint(player.leftHand, teleportAction, teleportHoldHint);
                        ShowButtonHint(player.rightHand, teleportAction, teleportHoldHint);
                        SetInstructionsOnCanvas(teleportInstructions);

                        if (teleportAction.GetStateDown(SteamVR_Input_Sources.Any))
                        {
                            HideAllHints();
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Teleport end
                    case 1:
                        ShowButtonHint(player.leftHand, teleportAction, teleportLetGoHint);
                        ShowButtonHint(player.rightHand, teleportAction, teleportLetGoHint);

                        if (teleportAction.GetStateUp(SteamVR_Input_Sources.Any))
                        {
                            HideAllHints();
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Grab
                    case 2:
                        SetInstructionsOnCanvas(pickUpInstructions);
                        foreach (Hand hand in player.hands)
                        {
                            if (hand.hoveringInteractable != null)
                            {
                                HideActionHints(hand, grabPinchAction);
                                ShowButtonHint(hand, grabPinchAction, grabHint);
                            }
                            else
                            {
                                HideActionHints(hand, grabPinchAction);
                                ShowButtonHint(hand, grabPinchAction, moveToGrabHint);
                            }

                            if (hand.AttachedObjects.Count > 0)
                            {
                                HideAllHints();
                                tutorialStep++;
                            }
                        }
                        break;
                    #endregion

                    #region Let go
                    case 3:
                        foreach (Hand hand in player.hands)
                        {
                            if (hand.AttachedObjects.Count > 0)
                            {
                                ShowButtonHint(hand, grabPinchAction, letGoHint);
                            }
                            else
                            {
                                HideActionHints(hand, grabPinchAction);
                            }
                        }
                        if (player.rightHand.AttachedObjects.Count + player.leftHand.AttachedObjects.Count == 0)
                        {
                            HideAllHints();
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Open menu
                    case 4:
                        ShowButtonHint(player.leftHand, menuAction, openMenuHint);
                        ShowButtonHint(player.rightHand, menuAction, openMenuHint);
                        SetInstructionsOnCanvas(openMenuInstructions);

                        if (IsMenuActive())
                        {
                            HideAllHints();
                            tutorialStep = interactWithMenuCheckpoint;
                        }
                        break;
                    #endregion

                    #region Interact with menu (play audio)
                    case 5:
                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }

                        SetInstructionsOnCanvas(playAudioInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);

                        menuLaser = GetMenuLaser(handWithLaser);

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "PlayAudioButton")
                        {
                            HideAllHints();
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Voice recognition
                    case 6:
                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }

                        SetInstructionsOnCanvas(voiceRecInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);

                        menuLaser = GetMenuLaser(handWithLaser);

                        // Continue if teacher. Done else
                        if (GameObject.FindGameObjectsWithTag("InteractableObject").Length > interactableObjectsInSceneAtStart)
                        {
                            HideAllHints();
                            tutorialStep = IsTeacher() ? tutorialStep + 1 : -1;
                        }
                        break;
                    #endregion

                    #region Spawn new object
                    case 7:
                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }


                        SetInstructionsOnCanvas(spawnObjectInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);

                        menuLaser = GetMenuLaser(handWithLaser);

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "AddInteractableObjectButton")
                        {
                            HideAllHints();
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Activate delete laser
                    case 8:
                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        // If object is deleted: continue tutorial
                        if (objectToInteractWith == null)
                        {
                            EnableAdditionalObjects(true);
                            tutorialStep += 2;
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }

                        SetInstructionsOnCanvas(deleteLaserInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);


                        menuLaser = GetMenuLaser(handWithLaser);

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "DeleteObjectButton")
                        {
                            HideAllHints();
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Delete object
                    case 9:
                        // If object is deleted: continue tutorial
                        if (objectToInteractWith == null)
                        {
                            EnableAdditionalObjects(true);
                            tutorialStep++;
                            break;
                        }

                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep - 1;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }

                        SetInstructionsOnCanvas(deleteInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);

                        menuLaser = GetMenuLaser(handWithLaser);

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "DeleteObjectButton")
                        {
                            HideAllHints();
                            tutorialStep--;
                        }
                        break;
                    #endregion

                    #region Activate edit laser
                    case 10:
                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }

                        SetInstructionsOnCanvas(editLaserInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);


                        menuLaser = GetMenuLaser(handWithLaser);

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "EditObjectButton")
                        {
                            HideAllHints();
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Edit object
                    case 11:
                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep - 1;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }

                        SetInstructionsOnCanvas(editInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);

                        menuLaser = GetMenuLaser(handWithLaser);

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "SettingsButton")
                        {
                            HideAllHints();
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Save state
                    case 12:
                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }

                        SetInstructionsOnCanvas(saveInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);

                        if (menuLaser == null)
                        {
                            menuLaser = GetMenuLaser(handWithLaser);
                        }

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "SaveStateButton")
                        {
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Open load state panel
                    case 13:
                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }

                        SetInstructionsOnCanvas(beginLoadInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);

                        if (menuLaser == null)
                        {
                            menuLaser = GetMenuLaser(handWithLaser);
                        }

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "LoadStateButton")
                        {
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Load/Delete state
                    case 14:
                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep - 1;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }

                        SetInstructionsOnCanvas(loadInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);

                        if (menuLaser == null)
                        {
                            menuLaser = GetMenuLaser(handWithLaser);
                        }

                        if (menuLaser.IsClickingButton() != null && (menuLaser.IsClickingButton().name == "LoadStateButton" || menuLaser.IsClickingButton().name == "DeleteStateButton"))
                        {
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Toggle text visibility
                    case 15:
                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }

                        SetInstructionsOnCanvas(toggleObjectTextInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);


                        if (menuLaser == null)
                        {
                            menuLaser = GetMenuLaser(handWithLaser);
                        }

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "ToggleTextButton")
                        {
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Invisibility
                    case 16:
                        previousHandWithLaser = handWithLaser;
                        handWithLaser = GetHandWithActiveLaser();

                        // If menu is not active (the user has closed the menu)
                        if (handWithLaser == null)
                        {
                            interactWithMenuCheckpoint = tutorialStep;
                            tutorialStep = menuStep;
                            HideAllHints();
                            break;
                        }

                        if (handWithLaser != previousHandWithLaser)
                        {
                            HideAllHints();
                        }

                        SetInstructionsOnCanvas(invisibleInstructions);
                        ShowButtonHintsMenuUI(handWithLaser);


                        if (menuLaser == null)
                        {
                            menuLaser = GetMenuLaser(handWithLaser);
                        }

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "InvisibilityButton")
                        {
                            tutorialStep = -1;
                        }
                        break;
                        #endregion
                }
                yield return null;
            }
            HideAllHints();
            SetInstructionsOnCanvas(exitGameInstructions);
        }

        private void EnableAdditionalObjects(bool toEnable)
        {
            foreach (GameObject go in objectsToSpawn)
                go.SetActive(toEnable);
        }

        private void SetInstructionsOnCanvas(string instructions)
        {
            string text = "";
            bool escape = false;
            foreach (char c in instructions)
            {
                if (c == '\\')
                {
                    escape = true;
                }
                else if (escape)
                {
                    escape = false;
                    if (c == 'n')
                    {
                        text += "\n";
                    }
                }
                else
                {
                    text += c;
                }
            }

            foreach (Text instructionText in instructionTexts)
                instructionText.text = text;
        }

        private MenuLaser GetMenuLaser(Hand laserHand)
        {
            string handWithLaserName = laserHand == player.leftHand ? "HandPrefabL" : "HandPrefabR";
            return TutorialGameManager.instance.GetPlayerAvatar().transform.Find(handWithLaserName).gameObject.GetComponent<MenuLaser>();
        }

        private void ShowButtonHintsMenuUI(Hand laserHand)
        {
            ShowButtonHint(laserHand, interactUIAction, deleteHint);
            ShowButtonHint(laserHand, menuAction, swapMenuHint);
            ShowButtonHint(laserHand, touchpadTouchedAction, scrollHint);
            ShowButtonHint(laserHand.otherHand, menuAction, closeMenuHint);
        }

        private void ShowButtonHint(Hand hand, SteamVR_Action_Boolean action, string text)
        {
            if (!ControllerButtonHints.IsButtonHintActive(hand, action))
            {
                ControllerButtonHints.ShowButtonHint(hand, action);
                ControllerButtonHints.ShowTextHint(hand, action, text, true);
            }
        }

        private void HideAllHints()
        {
            foreach (Hand hand in player.hands)
            {
                HideAllHints(hand);
            }
        }

        private void HideAllHints(Hand hand)
        {
            ControllerButtonHints.HideAllButtonHints(hand);
            ControllerButtonHints.HideAllTextHints(hand);
        }

        private void HideActionHints(Hand hand, SteamVR_Action_Boolean action)
        {
            ControllerButtonHints.HideButtonHint(hand, action);
            ControllerButtonHints.HideTextHint(hand, action);
        }

        private Hand GetHandWithActiveLaser()
        {
            Transform avatar = TutorialGameManager.instance.GetPlayerAvatar().transform;

            if (avatar.Find("HandPrefabL").gameObject.GetComponent<MenuLaser>().enabled)
            {
                return player.leftHand;
            }
            if (avatar.Find("HandPrefabR").gameObject.GetComponent<MenuLaser>().enabled)
            {
                return player.rightHand;
            }
            return null;
        }

        private bool IsMenuActive()
        {
            Transform leftHandMenu = player.leftHand.transform.Find("Menu");
            Transform rightHandMenu = player.rightHand.transform.Find("Menu");
            if (leftHandMenu != null && leftHandMenu.gameObject.activeInHierarchy)
            {
                return true;
            }
            if (rightHandMenu != null && rightHandMenu.gameObject.activeInHierarchy)
            {
                return true;
            }
            return false;
        }

        private bool IsTeacher()
        {
            object isTeacherObject;
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("admin", out isTeacherObject);
            if (isTeacherObject != null)
            {
                return (bool)isTeacherObject;
            }
            Debug.LogFormat("Admin mode not set already so set to {0} instead", false);
            return false;
        }

        private void SetDefaultStateIfNotSetAlready()
        {
            if (!EnvironmentState.StateExists(SceneManagerHelper.ActiveSceneName, EnvironmentState.DEFAULT_SAVE_NAME))
            {
                EnvironmentState.SaveEnvironmentState(SceneManagerHelper.ActiveSceneName, EnvironmentState.DEFAULT_SAVE_NAME);
            }
        }

        public void OnBackButtonClicked()
        {
            tutorialStep--;
        }
    }
}
