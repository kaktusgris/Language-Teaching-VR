using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using Photon.Voice.PUN;


namespace NTNU.CarloMarton.VRLanguage
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        [Tooltip("The prefab to use for representing the player")]
        public GameObject avatarPrefab;
     
        public GameObject interactablePrefab;

        public static GameManager Instance;


        [SerializeField] private String startScene = "TeleportTest";

        #region Photon Callbacks


        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {

            SceneManager.LoadScene(0);
        }

        public override void OnPlayerEnteredRoom(Player other)
        {

            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


                LoadArena();

            }

        }


        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


                LoadArena();
            }
        }

        #endregion


        #region Public Methods


        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void ExitGame()
        {
            PhotonNetwork.Disconnect();
            Application.Quit();
        }


        #endregion

        #region Private Methods

        private void Start()
        {

            Instance = this;
            
            if (PlayerManager.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);

                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                GameObject avatar = PhotonNetwork.Instantiate(this.avatarPrefab.name, ViveManager.Instance.head.transform.position, ViveManager.Instance.head.transform.rotation, 0);
                avatar.GetComponentInChildren<RandomColour>().newColour();
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }


            // Only connect the voice components if online
            if (PhotonNetwork.IsConnected)
            {
                GameObject.FindGameObjectWithTag("Voice").GetComponent<Recorder>().TransmitEnabled = true;
                GameObject.FindGameObjectWithTag("Voice").GetComponent<Recorder>().VoiceDetection = true;
            }
        }

        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", startScene);
            PhotonNetwork.LoadLevel(startScene);
         
        }

        private void instantiateObjects()
        {
            PhotonNetwork.Instantiate(this.interactablePrefab.name, new Vector3(1f, 1f, 1f), Quaternion.identity, 0);
        }


        #endregion
    }

}