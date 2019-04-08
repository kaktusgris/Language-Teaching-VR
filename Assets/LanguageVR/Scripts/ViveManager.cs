using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTNU.CarloMarton.VRLanguage
{
    public class ViveManager : MonoBehaviour {

        [Tooltip ("Player's VRCamera")]
        public GameObject head;

        [Tooltip("Player's LeftHand")]
        public GameObject leftHand;

        [Tooltip("Player's RightHand")]
        public GameObject rightHand;

        public static ViveManager Instance;

        // Run after steam VR transform on right hand to compensate for wrong transform on oculus touch controllers
        public void FixOculusTouchTransformRightHand()
        {
            if (UnityEngine.XR.XRDevice.model.IndexOf("Rift") >= 0)
            {
                rightHand.transform.Rotate(180, 0, 180);

                if (rightHand.transform.Find("RightRenderModel Slim(Clone)") != null)
                {
                    rightHand.transform.Find("RightRenderModel Slim(Clone)").localPosition = new Vector3(0, 0, 0.15f);
                    rightHand.transform.Find("ControllerButtonHints").localPosition = new Vector3(0, 0, 0.15f);
                    rightHand.transform.Find("HoverPoint").localPosition = Vector3.zero;
                }
            }
        }

        // Run after steam VR transform on left hand to compensate for wrong transform on oculus touch controllers
        public void FixOculusTouchTransformLeftHand()
        {
            if (UnityEngine.XR.XRDevice.model.IndexOf("Rift") >= 0)
            {
                leftHand.transform.Rotate(180, 0, 180);

                if (leftHand.transform.Find("LeftRenderModel Slim(Clone)") != null)
                {
                    leftHand.transform.Find("LeftRenderModel Slim(Clone)").localPosition = new Vector3(0, 0, 0.15f);
                    leftHand.transform.Find("ControllerButtonHints").localPosition = new Vector3(0, 0, 0.15f);
                    leftHand.transform.Find("HoverPoint").localPosition = Vector3.zero;
                }
            }
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this; 
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        } 
    }
}
