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
