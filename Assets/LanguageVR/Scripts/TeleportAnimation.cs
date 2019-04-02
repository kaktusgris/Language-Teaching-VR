using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace NTNU.CarloMarton.VRLanguage
{
    public class TeleportAnimation : MonoBehaviour, IPunObservable
    {
        public SteamVR_Action_Boolean teleportAction;
        public ParticleSystem animationObject;

        private bool visible = false;
        private Vector3 oldPosition = new Vector3();
        private Vector3 position;
        private Coroutine animationCoroutine;
        private PhotonView photonView;

        private void Awake()
        {
            photonView = gameObject.GetComponent<PhotonView>();
            UpdateActive();
            transform.SetParent(null);
        }

        private void Update()
        {
            if (photonView.IsMine)
            { 
                oldPosition = position;
                position = ViveManager.Instance.head.transform.position;
                if (Vector3.Distance(oldPosition, position) > 1)
                {
                    if (animationCoroutine != null)
                        StopCoroutine(animationCoroutine);

                    animationCoroutine = StartCoroutine(Animate(oldPosition, position));
                }
            }
        }

        private void UpdateActive()
        {
            if (photonView.IsMine)
                animationObject.Stop();
            else
            {
                if (visible)
                    animationObject.Play();
                else
                    animationObject.Stop();
            }
        }

        private IEnumerator Animate(Vector3 start, Vector3 end)
        {
            visible = true;

            Vector3 position = start;
            float maxDistanceDelta = 0.25f;

             while (position != end)
            {
                position = Vector3.MoveTowards(transform.position, end, maxDistanceDelta);
                transform.position = position;
                yield return null;
            }
            visible = false;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(visible);
                UpdateActive();
            }
            else
            {
                visible = (bool)stream.ReceiveNext();
                UpdateActive();
            }
        }
    }
}
