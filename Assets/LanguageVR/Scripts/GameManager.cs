using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;

namespace NTNU.CarloMarton.VRLanguage
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        [Tooltip("The prefab to use for representing the player")][SerializeField]
        private GameObject avatarPrefab;

        //public Valve.VR.InteractionSystem.Player playerInScene;

        private GameObject instantiatedAvatar;

        [SerializeField] private string startScene;

        #region Photon Callbacks


        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        #endregion

        #region Public Methods

        //-------------------------------------------------
        // Singleton instance of the GameManager. Only one can exist at a time.
        //-------------------------------------------------
        private static GameManager _instance;
        public static GameManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                }
                return _instance;
            }
        }

        public void LeaveRoom()
        {
            Destroy(GameObject.FindGameObjectWithTag("Player"));
            PhotonNetwork.LeaveRoom();

        }

        public void ExitGame()
        {
            PhotonNetwork.Disconnect();
            Application.Quit();
        }

        public GameObject GetPlayerAvatar()
        {
            return instantiatedAvatar;
        }

        #endregion

        #region Private Methods

        private void Start()
        {
            if (PlayerManager.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);

                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                instantiatedAvatar = PhotonNetwork.Instantiate(this.avatarPrefab.name, ViveManager.Instance.head.transform.position, ViveManager.Instance.head.transform.rotation, 0);

                // Make the local head invisible as to not see the inside of your own head
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
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }


            // Only connect the voice components if online
            if (PhotonNetwork.IsConnected)
            {
                Recorder voiceRecorder = GameObject.FindGameObjectWithTag("Voice").GetComponent<Recorder>();
                voiceRecorder.TransmitEnabled = true;
                GameObject.FindGameObjectWithTag("Voice").GetComponent<Photon.Voice.PUN.PhotonVoiceNetwork>().SpeakerPrefab = instantiatedAvatar;
            }
        }

        void Update()
        {
            //Resets objects that the host client and the scene owns
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.R))
            {
                print("Reloading level");
                
                LoadArena();
            }
        }

        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            else
            {
                Debug.LogFormat("PhotonNetwork : Loading Level : {0}", startScene);
                PhotonNetwork.LoadLevel(startScene);
            }

        }

        #endregion
    }

}
