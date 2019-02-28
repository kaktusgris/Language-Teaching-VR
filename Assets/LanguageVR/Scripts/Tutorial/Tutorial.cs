using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace NTNU.CarloMarton.VRLanguage
{
    public class Tutorial : MonoBehaviour
    {
        [Header ("Actions")]
        [SerializeField] private SteamVR_Action_Boolean grabPinchAction;
        [SerializeField] private SteamVR_Action_Boolean menuAction;
        [SerializeField] private SteamVR_Action_Boolean teleportAction;
        [SerializeField] private SteamVR_Action_Boolean interactUIAction;
        [SerializeField] private SteamVR_Action_Boolean touchpadTouchedAction;

        [Header("Instructions")]
        [SerializeField] private string teleportInstructions;
        [SerializeField] private string moveToGrabInstructions;
        [SerializeField] private string grabInstructions;
        [SerializeField] private string openMenuInstructions;
        [SerializeField] private string closeMenuInstructions;
        [SerializeField] private string interactWithmenuInstructions;
        [SerializeField] private string swapMenuInstructions;
        [SerializeField] private string scrollInstructions;

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
            int tutorialStep = 0;

            bool interactedWithMenu = false;
            Hand handWithLaser = GetHandWithActiveLaser();

            while (tutorialStep != -1)
            {
                Hand previousHandWithLaser = handWithLaser;
                handWithLaser = GetHandWithActiveLaser();

                foreach (Hand hand in player.hands)
                {
                    switch (tutorialStep)
                    {
                        #region case 0: Teleport
                        case 0:
                            ShowButtonHint(hand, teleportAction, teleportInstructions);

                            if (teleportAction.GetStateDown(hand.handType))
                            {
                                HideAllHints();
                                tutorialStep++;
                            }
                            break;
                        #endregion

                        #region Case 1: Grab
                        case 1:
                            if (hand.hoveringInteractable != null)
                            {
                                HideActionHints(hand, grabPinchAction);
                                ShowButtonHint(hand, grabPinchAction, grabInstructions);
                            }
                            else
                            {
                                HideActionHints(hand, grabPinchAction);
                                ShowButtonHint(hand, grabPinchAction, moveToGrabInstructions);
                            }

                            if (hand.AttachedObjects.Count > 0)
                            {
                                HideAllHints();
                                tutorialStep++;
                            }
                            break;
                        #endregion

                        #region Case 2: Open menu
                        case 2:
                            ShowButtonHint(hand, menuAction, openMenuInstructions);

                            if (IsMenuActive())
                            {
                                HideAllHints();
                                tutorialStep++;
                            }
                            break;
                        #endregion

                        #region Case 3: Interact with menu
                        case 3:
                            if (handWithLaser == null)
                            {
                                HideAllHints();
                                if (interactedWithMenu)
                                {
                                    tutorialStep = -1;
                                }
                                else
                                {
                                    tutorialStep--;
                                }
                                break;
                            }

                            if (hand == handWithLaser)
                            {
                                ShowButtonHint(hand, interactUIAction, interactWithmenuInstructions);
                                ShowButtonHint(hand, menuAction, swapMenuInstructions);
                                ShowButtonHint(hand, touchpadTouchedAction, scrollInstructions);
                                if (handWithLaser != previousHandWithLaser)
                                {
                                    HideAllHints();                                
                                }
                                string handWithLaserName = hand == player.leftHand ? "HandPrefabL" : "HandPrefabR";

                                MenuLaser menuLaser = GameManager.instance.GetAvatar().transform.Find(handWithLaserName).gameObject.GetComponent<MenuLaser>();
                                if (menuLaser.IsClinkingButton())
                                {
                                    interactedWithMenu = true;
                                }
                            }

                            if (hand == handWithLaser.otherHand)
                            {
                                ShowButtonHint(hand, menuAction, closeMenuInstructions);
                            }

                            break;
                            #endregion
                    }
                }
                yield return null;
            }
            HideAllHints();
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
            Transform avatar = GameManager.instance.GetAvatar().transform;

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
