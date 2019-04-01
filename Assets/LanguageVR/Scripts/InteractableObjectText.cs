﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace NTNU.CarloMarton.VRLanguage
{
    public class InteractableObjectText : MonoBehaviour
    {
        [SerializeField]
        private AudioClip audioClip;

        private ThrowableObject throwable;
        private MeshRenderer textRenderer;

        private void Start()
        {
            throwable = gameObject.GetComponentInParent<ThrowableObject>();
            textRenderer = gameObject.GetComponent<MeshRenderer>();
        }

        void Update()
        {
            // Only shows the text if the user is picking up the object
            if (throwable.IsAttached() && !textRenderer.enabled && throwable.IsMine() && (bool)PhotonNetwork.LocalPlayer.CustomProperties["ShowObjectText"])
            {
                textRenderer.enabled = true;
            }
            else if (!throwable.IsAttached() && textRenderer.enabled)
            {
                textRenderer.enabled = false;
            }

            // Rotate text to be seen by the user if it is visible
            if (textRenderer.enabled)
            {
                Transform headTransform = ViveManager.Instance.head.transform;
                gameObject.transform.rotation = Quaternion.LookRotation(gameObject.transform.position - headTransform.position);
            }
        }

        public void SetAudioClip(AudioClip audioClip)
        {
            this.audioClip = audioClip;
        }

        public AudioClip GetAudioClip()
        {
            return audioClip;
        }
    }
}
