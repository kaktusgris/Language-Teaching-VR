//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Basic throwable object
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Photon.Pun;
using TMPro;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	[RequireComponent( typeof( Rigidbody ) )]
    [RequireComponent( typeof(VelocityEstimator))]
	public class Throwable : MonoBehaviour, IPunObservable
    {
		[EnumFlags]
		[Tooltip( "The flags used to attach this object to the hand." )]
		public Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.ParentToHand | Hand.AttachmentFlags.DetachFromOtherHand | Hand.AttachmentFlags.TurnOnKinematic;

        [Tooltip("The local point which acts as a positional and rotational offset to use while held")]
        public Transform attachmentOffset;

		[Tooltip( "How fast must this object be moving to attach due to a trigger hold instead of a trigger press? (-1 to disable)" )]
        public float catchingSpeedThreshold = -1;

        public ReleaseStyle releaseVelocityStyle = ReleaseStyle.GetFromHand;

        [Tooltip("The time offset used when releasing the object with the RawFromHand option")]
        public float releaseVelocityTimeOffset = -0.011f;

        public float scaleReleaseVelocity = 1.1f;

		[Tooltip( "When detaching the object, should it return to its original parent?" )]
		public bool restoreOriginalParent = false;

        public bool attachEaseIn = false;
		public AnimationCurve snapAttachEaseInCurve = AnimationCurve.EaseInOut( 0.0f, 0.0f, 1.0f, 1.0f );
		public float snapAttachEaseInTime = 0.15f;

		protected VelocityEstimator velocityEstimator;
        protected bool attached = false;
        protected float attachTime;
        protected Vector3 attachPosition;
        protected Quaternion attachRotation;
        protected Transform attachEaseInTransform;

		public UnityEvent onPickUp;
		public UnityEvent onDetachFromHand;

		public bool snapAttachEaseInCompleted = false;
        
        protected RigidbodyInterpolation hadInterpolation = RigidbodyInterpolation.None;

        protected new Rigidbody rigidbody;

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

        [HideInInspector]
        public Interactable interactable;

        //-------------------------------------------------
        protected virtual void Awake()
		{
            velocityEstimator = GetComponent<VelocityEstimator>();
            interactable = GetComponent<Interactable>();

			if ( attachEaseIn )
			{
				attachmentFlags &= ~Hand.AttachmentFlags.SnapOnAttach;
			}

            rigidbody = GetComponent<Rigidbody>();
            rigidbody.maxAngularVelocity = 50.0f;


            if(attachmentOffset != null)
            {
                interactable.handFollowTransform = attachmentOffset;
            }

            photonView = GetComponent<PhotonView>();

            m_StoredPosition = transform.position;
            m_NetworkPosition = Vector3.zero;

            m_NetworkRotation = Quaternion.identity;

        }


        //-------------------------------------------------
        protected virtual void OnHandHoverBegin( Hand hand )
		{
			bool showHint = false;

            // "Catch" the throwable by holding down the interaction button instead of pressing it.
            // Only do this if the throwable is moving faster than the prescribed threshold speed,
            // and if it isn't attached to another hand
            if ( !attached && catchingSpeedThreshold != -1)
            {
                float catchingThreshold = catchingSpeedThreshold * SteamVR_Utils.GetLossyScale(Player.instance.trackingOriginTransform);

                GrabTypes bestGrabType = hand.GetBestGrabbingType();

                if ( bestGrabType != GrabTypes.None )
				{
					if (rigidbody.velocity.magnitude >= catchingThreshold)
					{
						hand.AttachObject( gameObject, bestGrabType, attachmentFlags );
						showHint = false;
					}
				}
			}

			if ( showHint )
			{
                hand.ShowGrabHint();
			}
		}


        //-------------------------------------------------
        protected virtual void OnHandHoverEnd( Hand hand )
		{
            hand.HideGrabHint();
		}


        //-------------------------------------------------
        protected virtual void HandHoverUpdate( Hand hand )
        {
            GrabTypes startingGrabType = hand.GetGrabStarting();
            
            if (startingGrabType != GrabTypes.None)
            {
				hand.AttachObject( gameObject, startingGrabType, attachmentFlags, attachmentOffset );
                hand.HideGrabHint();
            }
		}

        //-------------------------------------------------
        protected virtual void OnAttachedToHand( Hand hand )
		{
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

            hadInterpolation = this.rigidbody.interpolation;

            attached = true;

			onPickUp.Invoke();

			hand.HoverLock( null );
            
            rigidbody.interpolation = RigidbodyInterpolation.None;
            
		    velocityEstimator.BeginEstimatingVelocity();

			attachTime = Time.time;
			attachPosition = transform.position;
			attachRotation = transform.rotation;

			if ( attachEaseIn )
			{
                attachEaseInTransform = hand.objectAttachmentPoint;
			}

            timeLastInteraction = Time.time;

			snapAttachEaseInCompleted = false;
		}


        //-------------------------------------------------
        protected virtual void OnDetachedFromHand(Hand hand)
        {
            attached = false;

            onDetachFromHand.Invoke();

            hand.HoverUnlock(null);
            
            rigidbody.interpolation = hadInterpolation;

            Vector3 velocity;
            Vector3 angularVelocity;

            GetReleaseVelocities(hand, out velocity, out angularVelocity);

            rigidbody.velocity = velocity;
            rigidbody.angularVelocity = angularVelocity;

            timeLastInteraction = Time.time;
        }


        public virtual void GetReleaseVelocities(Hand hand, out Vector3 velocity, out Vector3 angularVelocity)
        {
            switch (releaseVelocityStyle)
            {
                case ReleaseStyle.ShortEstimation:
                    velocityEstimator.FinishEstimatingVelocity();
                    velocity = velocityEstimator.GetVelocityEstimate();
                    angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
                    break;
                case ReleaseStyle.AdvancedEstimation:
                    hand.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
                    break;
                case ReleaseStyle.GetFromHand:
                    velocity = hand.GetTrackedObjectVelocity(releaseVelocityTimeOffset);
                    angularVelocity = hand.GetTrackedObjectAngularVelocity(releaseVelocityTimeOffset);
                    break;
                default:
                case ReleaseStyle.NoChange:
                    velocity = rigidbody.velocity;
                    angularVelocity = rigidbody.angularVelocity;
                    break;
            }

            if (releaseVelocityStyle != ReleaseStyle.NoChange)
                velocity *= scaleReleaseVelocity;
        }

        //-------------------------------------------------
        protected virtual void HandAttachedUpdate(Hand hand)
        {
            if (attachEaseIn)
            {
                float t = Util.RemapNumberClamped(Time.time, attachTime, attachTime + snapAttachEaseInTime, 0.0f, 1.0f);
                if (t < 1.0f)
                {
                    t = snapAttachEaseInCurve.Evaluate(t);
                    transform.position = Vector3.Lerp(attachPosition, attachEaseInTransform.position, t);
                    transform.rotation = Quaternion.Lerp(attachRotation, attachEaseInTransform.rotation, t);
                }
                else if (!snapAttachEaseInCompleted)
                {
                    gameObject.SendMessage("OnThrowableAttachEaseInCompleted", hand, SendMessageOptions.DontRequireReceiver);
                    snapAttachEaseInCompleted = true;
                }
            }

            if (hand.IsGrabEnding(this.gameObject))
            {
                hand.DetachObject(gameObject, restoreOriginalParent);

                // Uncomment to detach ourselves late in the frame.
                // This is so that any vehicles the player is attached to
                // have a chance to finish updating themselves.
                // If we detach now, our position could be behind what it
                // will be at the end of the frame, and the object may appear
                // to teleport behind the hand when the player releases it.
                //StartCoroutine( LateDetach( hand ) );
            }
        }
        
        // Transfer ownership on other throwable objects that you touch with this throwable
        public void OnCollisionEnter(Collision collision)
        {
            Throwable otherThrowable = collision.collider.GetComponent<Throwable>();
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

        public void Update()
        {
            if (! this.photonView.IsMine)
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

        #region custom methods

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

        //-------------------------------------------------
        protected virtual IEnumerator LateDetach( Hand hand )
		{
			yield return new WaitForEndOfFrame();

			hand.DetachObject( gameObject, restoreOriginalParent );
		}


        //-------------------------------------------------
        protected virtual void OnHandFocusAcquired( Hand hand )
		{
			gameObject.SetActive( true );
			velocityEstimator.BeginEstimatingVelocity();
		}


        //-------------------------------------------------
        protected virtual void OnHandFocusLost( Hand hand )
		{
			gameObject.SetActive( false );
			velocityEstimator.FinishEstimatingVelocity();
		}
	}

    public enum ReleaseStyle
    {
        NoChange,
        GetFromHand,
        ShortEstimation,
        AdvancedEstimation,
    }
}
