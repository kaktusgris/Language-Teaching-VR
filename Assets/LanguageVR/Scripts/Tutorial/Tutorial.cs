using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace NTNU.CarloMarton.VRLanguage
{
    public class Tutorial : MonoBehaviour
    {
        public SteamVR_Action_Boolean grabPinchAction;
        public SteamVR_Action_Boolean menuAction;
        public SteamVR_Action_Boolean teleportAction;
        public SteamVR_Action_Boolean interactUIAction;

        private Valve.VR.InteractionSystem.Player player;
        private bool isTeacher;
        private Coroutine hintCoroutine = null;

        private bool hasShownTeleportHint = false;
        private bool hasShownGrabHint = false;
        private bool hasShownMenuHint = false;

        void Start()
        {
            player = Valve.VR.InteractionSystem.Player.instance;
            isTeacher = IsTeacher();

            StartCoroutine(HintManagerCoroutine());
        }

        private IEnumerator HintManagerCoroutine()
        {
            int activeHint = 0;

            while (activeHint != -1)
            {
                foreach (Hand hand in player.hands)
                {
                    switch (activeHint)
                    {
                        // Teleport
                        case 0:
                            ShowButtonHint(hand, teleportAction, "Teleport");

                            if (teleportAction.GetLastStateDown(SteamVR_Input_Sources.Any))
                            {
                                activeHint++;
                            }
                            break;

                        // Grab
                        case 1:
                            ShowButtonHint(hand, grabPinchAction, "Grab");

                            if (hand.AttachedObjects.Count > 0)
                            {
                                activeHint++;
                            }
                            break;

                        // Open menu
                        case 2:
                            ShowButtonHint(hand, menuAction, "Open menu");

                            if (menuAction.GetLastStateDown(SteamVR_Input_Sources.Any))
                            {
                                activeHint++;
                            }
                            break;

                        // Interact with menu
                        case 3:
                            Hand handWithLaser = GetHandWithActiveLaser();
                            Hand handWithMenu = null;
                            SteamVR_Input_Sources laserHand = SteamVR_Input_Sources.Any;
                            SteamVR_Input_Sources menuHand = SteamVR_Input_Sources.Any;

                            if (handWithLaser.Equals(player.leftHand))
                            {
                                handWithMenu = player.rightHand;
                                laserHand = SteamVR_Input_Sources.LeftHand;
                                menuHand = SteamVR_Input_Sources.RightHand;
                            }
                            else if (handWithLaser.Equals(player.rightHand))
                            {
                                handWithMenu = player.leftHand;
                                laserHand = SteamVR_Input_Sources.RightHand;
                                menuHand = SteamVR_Input_Sources.LeftHand;
                            }

                            if (hand == handWithLaser)
                            {
                                ShowButtonHint(handWithLaser, interactUIAction, "Interact with menu");
                            }
                            if (hand == handWithMenu)
                            {
                                ShowButtonHint(handWithMenu, menuAction, "Close menu");
                            }

                            if (laserHand != SteamVR_Input_Sources.Any && interactUIAction.GetLastStateDown(menuHand))
                            {
                                activeHint = -1;
                            }
                            break;
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
                ControllerButtonHints.HideAllButtonHints(hand);
                ControllerButtonHints.HideAllTextHints(hand);
                ControllerButtonHints.ShowButtonHint(hand, action);
                ControllerButtonHints.ShowTextHint(hand, action, text, true);
            }
        }

        private void HideAllHints()
        {
            foreach (Hand hand in player.hands)
            {
                ControllerButtonHints.HideAllButtonHints(hand);
                ControllerButtonHints.HideAllTextHints(hand);
            }
        }

        private Hand GetHandWithActiveLaser()
        {
            Transform avatar = GameManager.instance.GetAvatar().transform;

            if (avatar.Find("HandPrefabL").gameObject.GetComponent<MenuLaser>().enabled)
            {
                return player.leftHand;
            }
            else if (avatar.Find("HandPrefabR").gameObject.GetComponent<MenuLaser>().enabled)
            {
                return player.rightHand;
            }
            else
            {
                return null;
            }
        }

        private bool Teleporting(Hand hand)
        {
            if (!hasShownTeleportHint)
            {
                if (!string.IsNullOrEmpty(ControllerButtonHints.GetActiveHintText(hand, teleportAction)))
                {
                    print(ControllerButtonHints.GetActiveHintText(hand, teleportAction));
                    hasShownTeleportHint = true;
                }
            }
            else if (string.IsNullOrEmpty(ControllerButtonHints.GetActiveHintText(hand, teleportAction)))
            {
                return false;
            }
            return true;
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

        private bool HasHand(Hand hand)
        {
            return hand.gameObject.activeInHierarchy;
        }

        private IEnumerator GrabHintCoroutine(SteamVR_Action_Boolean action, string hint)
        {
            float prevBreakTime = Time.time;
            float prevHapticPulseTime = Time.time;

            while (true)
            {
                bool pulsed = false;

                //Show the hint on each eligible hand
                foreach (Hand hand in player.hands)
                {
                    bool showHint = hand.hoveringInteractable;
                    bool isShowingHint = !string.IsNullOrEmpty(ControllerButtonHints.GetActiveHintText(hand, action));
                    if (showHint)
                    {
                        if (!isShowingHint)
                        {
                            ControllerButtonHints.ShowTextHint(hand, grabPinchAction, "Grab");
                            prevBreakTime = Time.time;
                            prevHapticPulseTime = Time.time;
                        }

                        if (Time.time > prevHapticPulseTime + 0.05f)
                        {
                            //Haptic pulse for a few seconds
                            pulsed = true;

                            hand.TriggerHapticPulse(500);
                        }
                    }
                    else if (!showHint && isShowingHint)
                    {
                        ControllerButtonHints.HideTextHint(hand, grabPinchAction);
                    }
                }

                if (Time.time > prevBreakTime + 3.0f)
                {
                    //Take a break for a few seconds
                    yield return new WaitForSeconds(3.0f);

                    prevBreakTime = Time.time;
                }

                if (pulsed)
                {
                    prevHapticPulseTime = Time.time;
                }

                yield return null;
            }
        }
    }
}
