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
            if (photonView.IsMine || !PhotonNetwork.IsConnected)
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
            
            if (!photonView.IsMine && PhotonNetwork.IsConnected)
            {
                Destroy(GetComponentInChildren<AudioListener>());
            }

            if (photonView.IsMine)
            {
                gameObject.GetComponentInChildren<AudioSource>().spatialBlend = 1.0f;
            }

        }

        #endregion

        void Update()
        {
            if (photonView.IsMine || !PhotonNetwork.IsConnected) { 

                if (GameObject.Find("Voice(Clone)").GetComponent<Recorder>().VoiceDetector.Detected)
                {
                    voiceDetected = true;
                } else
                {
                    voiceDetected = false;
                }
            }


        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(voiceDetected);
            }
            else
            {
                voiceIndicator.GetComponent<MeshRenderer>().enabled = (bool) stream.ReceiveNext();
            }
        }
    }
}
