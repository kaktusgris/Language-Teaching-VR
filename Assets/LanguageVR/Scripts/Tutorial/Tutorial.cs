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
        [SerializeField] private Text instructionText;
        [SerializeField] private GameObject objectToInteractWith;

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
        [SerializeField] private string spawnObjectInstructions = "Legg til et nytt objekt i verden ved å trykke på plussikonet.\nEt nytt objekt av typen du trykker på vil da dukke opp i verden.";
        [SerializeField] private string deleteLaserInstructions = "Slett kameraet ved å trykke på søppelkasseikonet. Dette vil endre fargen på laseren til rød og kan nå slette ting.";
        [SerializeField] private string deleteInstructions = "Fullfør sletting av kamera ved å peke laseren på kameraet for så å holde nede klikkeknappen i to sekunder.\nDu vil da se et vindu komme opp og etter de to sekundene vil kameraet forsvinne.\nAlle objekter som kan interageres med kan slettes.";
        [SerializeField] private string invisibleInstructions = "Bli usynlig ved å trykke på øyeikonet.\nSå lenge du er usynlig vil ingen andre kunne se deg, men du kan fortsatt interagere med objekter og observere.\nTrykk på knappen igjen for å bli synlig igjen.";
        [SerializeField] private string exitGameInstructions = "Avslutt spillet ved å trykke på krysset.\n -------- \nExit the game by pressing the cross.";

        private Valve.VR.InteractionSystem.Player player;
        private bool isTeacher;

        void Start()
        {
            player = Valve.VR.InteractionSystem.Player.instance;
            isTeacher = IsTeacher();

            StartCoroutine(HintManagerCoroutine());
        }

        

        private IEnumerator HintManagerCoroutine()
        {
            int menuStep = 4;

            int tutorialStep = 0;
            int interactWithMenuCheckpoint = menuStep + 1;

            Hand handWithLaser = GetHandWithActiveLaser();
            Hand previousHandWithLaser = null;
            string handWithLaserName = "";
            MenuLaser menuLaser = null;

            while (tutorialStep != -1)
            {
                switch (tutorialStep)
                {
                    #region case 0: Teleport start
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

                    #region Case 1: Teleport end
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

                    #region Case 2: Grab
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

                    #region Case 3: Let go
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

                    #region Case 4: Open menu
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

                    #region Case 5: Interact with menu (play audio)
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
                        ShowButtonHint(handWithLaser, interactUIAction, interactWithMenuHint);
                        ShowButtonHint(handWithLaser, menuAction, swapMenuHint);
                        ShowButtonHint(handWithLaser, touchpadTouchedAction, scrollHint);
                        ShowButtonHint(handWithLaser.otherHand, menuAction, closeMenuHint);

                        handWithLaserName = handWithLaser == player.leftHand ? "HandPrefabL" : "HandPrefabR";
                        menuLaser = TutorialGameManager.instance.GetPlayerAvatar().transform.Find(handWithLaserName).gameObject.GetComponent<MenuLaser>();

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "PlayAudioButton")
                        {
                            HideAllHints();
                            tutorialStep = isTeacher ? tutorialStep + 1 : -1;
                        }
                        break;
                    #endregion

                    #region Case 6: Spawn new object
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


                        SetInstructionsOnCanvas(spawnObjectInstructions);
                        ShowButtonHint(handWithLaser, interactUIAction, interactWithMenuHint);
                        ShowButtonHint(handWithLaser, menuAction, swapMenuHint);
                        ShowButtonHint(handWithLaser, touchpadTouchedAction, scrollHint);
                        ShowButtonHint(handWithLaser.otherHand, menuAction, closeMenuHint);

                        handWithLaserName = handWithLaser == player.leftHand ? "HandPrefabL" : "HandPrefabR";
                        menuLaser = TutorialGameManager.instance.GetPlayerAvatar().transform.Find(handWithLaserName).gameObject.GetComponent<MenuLaser>();

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "AddInteractableObjectButton")
                        {
                            HideAllHints();
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Case 7: Activate delete laser
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

                        SetInstructionsOnCanvas(deleteLaserInstructions);
                        ShowButtonHint(handWithLaser, interactUIAction, interactWithMenuHint);
                        ShowButtonHint(handWithLaser, menuAction, swapMenuHint);
                        ShowButtonHint(handWithLaser, touchpadTouchedAction, scrollHint);
                        ShowButtonHint(handWithLaser.otherHand, menuAction, closeMenuHint);


                        handWithLaserName = handWithLaser == player.leftHand ? "HandPrefabL" : "HandPrefabR";
                        menuLaser = TutorialGameManager.instance.GetPlayerAvatar().transform.Find(handWithLaserName).gameObject.GetComponent<MenuLaser>();

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "DeleteObjectButton")
                        {
                            HideAllHints();
                            tutorialStep++;
                        }
                        break;
                    #endregion

                    #region Case 8: Delete object
                    case 8:
                        if (objectToInteractWith == null)
                        {
                            tutorialStep++;
                            break;
                        }

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

                        SetInstructionsOnCanvas(deleteInstructions);
                        ShowButtonHint(handWithLaser, interactUIAction, deleteHint);
                        ShowButtonHint(handWithLaser, menuAction, swapMenuHint);
                        ShowButtonHint(handWithLaser, touchpadTouchedAction, scrollHint);
                        ShowButtonHint(handWithLaser.otherHand, menuAction, closeMenuHint);

                        handWithLaserName = handWithLaser == player.leftHand ? "HandPrefabL" : "HandPrefabR";
                        menuLaser = TutorialGameManager.instance.GetPlayerAvatar().transform.Find(handWithLaserName).gameObject.GetComponent<MenuLaser>();

                        if (menuLaser.IsClickingButton() != null && menuLaser.IsClickingButton().name == "DeleteObjectButton")
                        {
                            HideAllHints();
                            tutorialStep--;
                        }
                        break;
                    #endregion

                    #region Case 9: Invisibility
                    case 9:
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
                        ShowButtonHint(handWithLaser, interactUIAction, interactWithMenuHint);
                        ShowButtonHint(handWithLaser, menuAction, swapMenuHint);
                        ShowButtonHint(handWithLaser, touchpadTouchedAction, scrollHint);
                        ShowButtonHint(handWithLaser.otherHand, menuAction, closeMenuHint);


                        if (menuLaser == null)
                        {
                            handWithLaserName = handWithLaser == player.leftHand ? "HandPrefabL" : "HandPrefabR";
                            menuLaser = TutorialGameManager.instance.GetPlayerAvatar().transform.Find(handWithLaserName).gameObject.GetComponent<MenuLaser>();
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
            instructionText.text = text;
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
    }
}
