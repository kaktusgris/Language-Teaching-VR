using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections;
using Photon.Pun;
using Photon.Voice.Unity;
using Photon.Voice.PUN;

namespace NTNU.CarloMarton.VRLanguage
{
    /// <summary>
    /// Player manager.
    /// </summary>
    ///
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {

        public GameObject voiceIndicator;
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


        }

        void Start()
        {
            voiceIndicator.GetComponent<MeshRenderer>().enabled = false;
            if (!photonView.IsMine && PhotonNetwork.IsConnected && photonView.Owner != null)
            {
                print("Audio listener destroyed");
                Destroy(GetComponentInChildren<AudioListener>());
            }

            if (photonView.IsMine)
            {
                gameObject.GetComponentInChildren<AudioSource>().spatialBlend = 1.0f;
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
        private void ChangeHands()
        {
            SkinnedMeshRenderer rightRenderer = GameObject.FindGameObjectWithTag("MainCamera").transform.Find("RightHand").Find("RightRenderModel Slim(Clone)").Find("vr_glove_right_model_slim(Clone)").Find("slim_r").Find("vr_glove_right_slim").gameObject.GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer leftRenderer = GameObject.FindGameObjectWithTag("MainCamera").transform.Find("LeftHand").Find("LeftRenderModel Slim(Clone)").Find("vr_glove_left_model_slim(Clone)").Find("slim_l").Find("vr_glove_right_slim").gameObject.GetComponent<SkinnedMeshRenderer>();
            handsChanged = !handsChanged;

            Shader newShader;

            newShader = handsChanged ? Shader.Find("VR/SpatialMapping/Wireframe") : Shader.Find("Standard");

            rightRenderer.material.shader = newShader;
            leftRenderer.material.shader = newShader;
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
                    ChangeHands();
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
