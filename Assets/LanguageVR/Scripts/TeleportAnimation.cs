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

        private bool toPlay = false;
        private bool visible = true;
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
                if (toPlay && visible)
                    animationObject.Play();
                else
                    animationObject.Stop();
            }
        }

        private IEnumerator Animate(Vector3 start, Vector3 end)
        {
            toPlay = true;

            Vector3 position = start;
            float maxDistanceDelta = 0.25f;

             while (position != end)
            {
                position = Vector3.MoveTowards(transform.position, end, maxDistanceDelta);
                transform.position = position;
                yield return null;
            }
            toPlay = false;
        }

        public void SetVisible(bool visible)
        {
            this.visible = visible;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(toPlay);
                stream.SendNext(visible);
                UpdateActive();
            }
            else
            {
                toPlay = (bool)stream.ReceiveNext();
                visible = (bool)stream.ReceiveNext();
                UpdateActive();
            }
        }
    }
}
