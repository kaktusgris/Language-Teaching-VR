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
    public class PlayerManager : MonoBehaviourPunCallbacks
    {

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #region Private Fields


        #endregion

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
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);


        }

        void Start()
        {
            if (!photonView.IsMine)
            {
                Destroy(GetComponentInChildren<AudioListener>());
            }

            if (photonView.IsMine)
            {
                gameObject.GetComponentInChildren<AudioSource>().spatialBlend = 1.0f;
                gameObject.GetComponentInChildren<AudioSource>().rolloffMode = AudioRolloffMode.Logarithmic;
            }
            
        }


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {
            
            //ProcessInputs();
        }

        #endregion

        #region Custom

        void ProcessInputs()
        {
            
        }

        #endregion
    }
}
