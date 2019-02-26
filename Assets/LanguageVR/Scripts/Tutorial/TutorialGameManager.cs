﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTNU.CarloMarton.VRLanguage
{
    public class TutorialGameManager : MonoBehaviour
    {
        [Tooltip("The prefab to use for representing the player")]
        public GameObject avatarPrefab;

        public static TutorialGameManager Instance;

        private GameObject instantiatedAvatar;


        public GameObject GetPlayer()
        {
            return instantiatedAvatar;
        }

        private void Start()
        {
            Instance = this;

            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            instantiatedAvatar = Instantiate(this.avatarPrefab, ViveManager.Instance.head.transform.position, ViveManager.Instance.head.transform.rotation);

            // Make the head invisible as to not see the inside of your own head
            try
            {
                Transform head = instantiatedAvatar.transform.Find("Body").transform.Find("Head");
                head.GetComponent<MeshRenderer>().enabled = false;
                head.Find("NameTag").GetComponent<MeshRenderer>().enabled = false;
                head.Find("VoiceIndicator").GetComponent<MeshRenderer>().enabled = false;
            }
            catch (NullReferenceException)
            {
                Debug.LogError("NullReferenceException. Probably because the head component in avatar is not named Head");
            }
        }
    }
}
