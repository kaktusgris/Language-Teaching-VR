using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections;
using Photon.Pun;
using Photon.Voice.Unity;
using Photon.Voice.PUN;
using UnityEngine.SceneManagement;
using Valve.VR.InteractionSystem;

namespace NTNU.CarloMarton.VRLanguage
{
    /// <summary>
    /// Player manager.
    /// </summary>
    ///
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {

        public GameObject voiceIndicator;
        public string[] scenesToHaveVisibleControllers;

        private bool voiceDetected = false;
        private bool visible = true;
        private bool handsChanged = false;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {
                PlayerManager.LocalPlayerInstance = this.gameObject;
                GameObject.Find("Voice(Clone)").GetComponent<Recorder>().VoiceDetectorCalibrate(200);
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);

            // Teleport has hint by default, disable it here to not show up
            Valve.VR.InteractionSystem.Teleport.instance.CancelTeleportHint();
        }

        private IEnumerator Start()
        {
            voiceIndicator.GetComponent<MeshRenderer>().enabled = false;
            if (!photonView.IsMine && PhotonNetwork.IsConnected && photonView.Owner != null)
            {
                print("Audio listener destroyed");
                Destroy(GetComponentInChildren<AudioListener>());
            }

			if (photonView.IsMine || !PhotonNetwork.IsConnected)
			{
				PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "ShowObjectText", true } });
			}

            if (photonView.IsMine)
            {
                gameObject.GetComponentInChildren<AudioSource>().spatialBlend = 1.0f;
            }

            // Hands only exists if controllers are tracked, so we look for them until they are found
            foreach (string sceneName in scenesToHaveVisibleControllers)
            {
                if (sceneName.Equals(SceneManager.GetActiveScene().name))
                {
                    print(SceneManager.GetActiveScene().name);
                    yield break;
                }
            }

            bool lHandChanged = false;
            bool rHandChanged = false;
            Hand lHand = Player.instance.leftHand;
            Hand rHand = Player.instance.rightHand;

            while (true)
            {
                if (!lHandChanged && lHand.transform.Find("LeftRenderModel Slim(Clone)"))
                {
                    HideController(lHand);
                    lHandChanged = true;
                }
                if (!rHandChanged && rHand.transform.Find("RightRenderModel Slim(Clone)"))
                {
                    HideController(rHand);
                    rHandChanged = true;
                }
                if (lHandChanged && rHandChanged)
                {
                    break;
                }

                yield return null;
            }
        }

        #endregion


        public void SetVisibility(bool visible)
        {
            this.visible = visible;
            SetCollisionOnHead(visible);
        }

        // Toggle the meshrenders of the avatar, except it's voice indicator as that is handled differently
        private void SetIfRender(bool toRender)
        {
            foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                if (!renderer.Equals(voiceIndicator.GetComponent<MeshRenderer>()))
                {
                    renderer.enabled = toRender;
                }
            }
            foreach (SkinnedMeshRenderer renderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                renderer.enabled = toRender;
            }
        }

        private void SetCollisionOnHead(bool collision)
        {
            SphereCollider headCollider = gameObject.GetComponentInChildren<SphereCollider>();
            headCollider.enabled = collision;
        }

        // Toggle the shader on hands between standard and a wireframe look
        private void ToggleWireframeOnHands()
        {
            Player player = Player.instance;

            SkinnedMeshRenderer rightRenderer = player.rightHand.transform.Find("RightRenderModel Slim(Clone)/vr_glove_right_model_slim(Clone)/slim_r/vr_glove_right_slim").gameObject.GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer leftRenderer = player.leftHand.transform.Find("LeftRenderModel Slim(Clone)/vr_glove_left_model_slim(Clone)/slim_l/vr_glove_right_slim").gameObject.GetComponent<SkinnedMeshRenderer>();
            handsChanged = !handsChanged;

            Shader newShader;

            newShader = handsChanged ? Shader.Find("VR/SpatialMapping/Wireframe") : Shader.Find("Standard");

            rightRenderer.material.shader = newShader;
            leftRenderer.material.shader = newShader;
        }

        public void HideController(Hand hand)
        {
            hand.HideController(true);
            hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithoutController);
        }

        public void ShowController(Hand hand)
        {
            hand.HideController(false);
            hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithController);
        }

        void Update()
        {
            if (photonView.IsMine || !PhotonNetwork.IsConnected) {
                if (PhotonNetwork.IsConnected && GameObject.Find("Voice(Clone)").GetComponent<Recorder>().VoiceDetector.Detected)
                {
                    voiceDetected = true;
                } else
                {
                    voiceDetected = false;
                }

                if (visible && handsChanged || !visible && !handsChanged)
                {
                    ToggleWireframeOnHands();
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(visible);
                stream.SendNext(voiceDetected);
            }
            else
            {
                bool photonVisible = (bool) stream.ReceiveNext();
                bool photonVoiceDetected = (bool) stream.ReceiveNext();

                voiceIndicator.GetComponent<MeshRenderer>().enabled = photonVoiceDetected && photonVisible;
                SetIfRender(photonVisible);
                SetCollisionOnHead(photonVisible);
            }
        }
    }
}
