using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace NTNU.CarloMarton.VRLanguage
{
    public class ThrowableObject : Valve.VR.InteractionSystem.Throwable, IPunObservable
    {
        private PhotonView photonView;

        private double timeLastInteraction = 0.0d;

        private float m_Distance;
        private float m_Angle;

        private Vector3 m_Direction;
        private Vector3 m_NetworkPosition;
        private Vector3 m_StoredPosition;

        private Quaternion m_NetworkRotation;

        public bool m_SynchronizePosition = true;
        public bool m_SynchronizeRotation = true;
        public bool m_SynchronizeScale = false;


        protected override void Awake()
        {
            base.Awake();

            photonView = GetComponent<PhotonView>();

            m_StoredPosition = transform.position;
            m_NetworkPosition = Vector3.zero;
            m_NetworkRotation = Quaternion.identity;
        }

        public void Update()
        {
            if (!IsMine())
            {
                rigidbody.isKinematic = true;

                transform.position = Vector3.MoveTowards(transform.position, this.m_NetworkPosition, this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, this.m_NetworkRotation, this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
            }
            else
            {
                rigidbody.isKinematic = attached;
            }
        }

        protected override void OnAttachedToHand(Hand hand)
        {
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

            base.OnAttachedToHand(hand);

            timeLastInteraction = Time.time;

            //Add this gameobject to dictionary when it is picked up by the player.
            GameObject playerAvatar;
            if (GameManager.instance != null)
                playerAvatar = GameManager.instance.GetPlayerAvatar();
            else
                playerAvatar = TutorialGameManager.instance.GetPlayerAvatar();

            PlayerDictionary dictionary = playerAvatar.GetComponent<PlayerDictionary>();
            string name = this.gameObject.GetComponentInChildren<TextMesh>(true).text;

            // tries to add item to dictionary when picked up. Plays the item's audioClip if it is added (for the first time)
            if (dictionary.AddItemToDictionary(name, this.gameObject))
            {
                dictionary.PlayAudio(name);
            }
        }

        protected override void OnDetachedFromHand(Hand hand)
        {
            base.OnDetachedFromHand(hand);

            timeLastInteraction = Time.time;
        }

        #region new methods

        // Transfer ownership on other throwable objects that you touch with this throwable
        public void OnCollisionEnter(Collision collision)
        {
            ThrowableObject otherThrowable = collision.collider.GetComponent<ThrowableObject>();
            PhotonView otherPhotonView = collision.collider.GetComponent<PhotonView>();

            if (otherThrowable == null || otherPhotonView == null || this.photonView.Owner == otherPhotonView.Owner)
            {
                // The other object is not interactable or has the same owner
                return;
            }

            if (this.attached)
            {
                otherPhotonView.TransferOwnership(this.photonView.Owner);
                otherThrowable.timeLastInteraction = Time.time;
            }
            else if (otherThrowable.attached)
            {
                this.photonView.TransferOwnership(otherPhotonView.Owner);
                this.timeLastInteraction = Time.time;
            }
            else if (otherThrowable.timeLastInteraction < this.timeLastInteraction)
            {
                otherPhotonView.TransferOwnership(this.photonView.Owner);
                otherThrowable.timeLastInteraction = Time.time;
            }
            else
            {
                this.photonView.TransferOwnership(otherPhotonView.Owner);
                this.timeLastInteraction = Time.time;
            }
        }

        public bool IsMine()
        {
            return photonView.IsMine || !PhotonNetwork.IsConnected;
        }

        public bool IsAttached()
        {
            return attached;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                this.m_Direction = transform.position - this.m_StoredPosition;
                this.m_StoredPosition = transform.position;

                stream.SendNext(transform.position);
                stream.SendNext(this.m_Direction);
                stream.SendNext(transform.rotation);

                stream.SendNext(this.attached);
                stream.SendNext(this.timeLastInteraction);
            }
            else
            {
                this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                this.m_Direction = (Vector3)stream.ReceiveNext();

                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.timestamp));
                this.m_NetworkPosition += this.m_Direction * lag;

                this.m_Distance = Vector3.Distance(transform.position, this.m_NetworkPosition);
                this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();
                this.m_Angle = Quaternion.Angle(transform.rotation, this.m_NetworkRotation);

                this.attached = (bool)stream.ReceiveNext();
                this.timeLastInteraction = (double)stream.ReceiveNext();
            }
        }

        #endregion
    }
}