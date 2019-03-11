using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NTNU.CarloMarton.VRLanguage
{
    public class MainPanel : MonoBehaviourPunCallbacks
    {
        [Tooltip("The prefab for Voice")]
        public GameObject Voice;

        public string sceneToLoadString = "Forest";
        public string tutorialToLoadString = "Tutorial";

        [Header("Login Panel")]
        public GameObject LoginPanel;

        public InputField PlayerNameInput;
        public InputField AdminPasswordInput;
        public string adminPassword;

        [Header("Selection Panel")]
        public GameObject SelectionPanel;

        [Header("Create Room Panel")]
        public GameObject CreateRoomPanel;

        public InputField RoomNameInputField;
        public InputField MaxPlayersInputField;

        [Header("Join Random Room Panel")]
        public GameObject JoinRandomRoomPanel;

        [Header("Room List Panel")]
        public GameObject RoomListPanel;

        public GameObject RoomListContent;
        public GameObject RoomListEntryPrefab;

        [Header("Inside Room Panel")]
        public GameObject InsideRoomPanel;
        public Button StartGameButton;
        public GameObject PlayerListEntryPrefab;
        public Text roomNameText;

        [Header("Loading panel")]
        public GameObject LoadingPanel;
        public Text LoadingText;

        [Header("Tutorial Panel")]
        public GameObject TutorialPanel;
        public InputField TutorialAdminPasswordInput;

        private Dictionary<string, RoomInfo> cachedRoomList;
        private Dictionary<string, GameObject> roomListEntries;
        private Dictionary<int, GameObject> playerListEntries;

        private bool adminMode = false;

        

        string gameVersion = "1";

        #region UNITY

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            if (GameObject.FindGameObjectWithTag("Voice") == null)
            {
                Instantiate(Voice, new Vector3(0, 0, 0), Quaternion.identity);
                GameObject.FindGameObjectWithTag("Voice").GetComponent<Recorder>().MicrophoneType = Recorder.MicType.Unity;
            }

            cachedRoomList = new Dictionary<string, RoomInfo>();
            roomListEntries = new Dictionary<string, GameObject>();
        }

        #endregion

        #region PUN CALLBACKS

        public override void OnConnectedToMaster()
        {
            this.SetActivePanel(SelectionPanel.name);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            ClearRoomListView();

            UpdateCachedRoomList(roomList);
            UpdateRoomListView();
        }

        public override void OnLeftLobby()
        {
            cachedRoomList.Clear();

            ClearRoomListView();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            SetActivePanel(SelectionPanel.name);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            SetActivePanel(SelectionPanel.name);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            string roomName = "Room " + Random.Range(1000, 10000);

            RoomOptions options = new RoomOptions { MaxPlayers = 8 };

            PhotonNetwork.CreateRoom(roomName, options, null);
        }

        public override void OnJoinedRoom()
        {
            SetActivePanel(InsideRoomPanel.name);

            roomNameText.text = PhotonNetwork.CurrentRoom.Name;

            if (playerListEntries == null)
            {
                playerListEntries = new Dictionary<int, GameObject>();
            }

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                GameObject entry = Instantiate(PlayerListEntryPrefab);
                entry.transform.SetParent(InsideRoomPanel.transform);
                entry.transform.localScale = Vector3.one;
                entry.GetComponent<PlayerEntry>().Initialize(p.ActorNumber, p.NickName);

                object isPlayerReady;
                if (p.CustomProperties.TryGetValue("IsPlayerReady", out isPlayerReady))
                {
                    entry.GetComponent<PlayerEntry>().SetPlayerReady((bool)isPlayerReady);
                }

                playerListEntries.Add(p.ActorNumber, entry);
            }

            StartGameButton.gameObject.SetActive(CheckPlayersReady());

            Hashtable props = new Hashtable
            {
                {"PlayerLoadedLevel", false}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public override void OnLeftRoom()
        {
            SetActivePanel(SelectionPanel.name);

            foreach (GameObject entry in playerListEntries.Values)
            {
                Destroy(entry.gameObject);
            }

            playerListEntries.Clear();
            playerListEntries = null;
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            GameObject entry = Instantiate(PlayerListEntryPrefab);
            entry.transform.SetParent(InsideRoomPanel.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<PlayerEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

            playerListEntries.Add(newPlayer.ActorNumber, entry);

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
            playerListEntries.Remove(otherPlayer.ActorNumber);

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
            {
                StartGameButton.gameObject.SetActive(CheckPlayersReady());
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (playerListEntries == null)
            {
                playerListEntries = new Dictionary<int, GameObject>();
            }

            GameObject entry;
            if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
            {
                object isPlayerReady;
                if (changedProps.TryGetValue("IsPlayerReady", out isPlayerReady))
                {
                    entry.GetComponent<PlayerEntry>().SetPlayerReady((bool)isPlayerReady);
                }
            }

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }


        #endregion

        #region UI CALLBACKS

        public void OnBackButtonClicked()
        {
            SetActivePanel(SelectionPanel.name);
        }

        public void OnCreateRoomButtonClicked()
        {
            string roomName = RoomNameInputField.text;
            roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

            byte maxPlayers;
            byte.TryParse(MaxPlayersInputField.text, out maxPlayers);

            if (maxPlayers < 2 || maxPlayers > 8)
            {
                // The user entered 0 or not a number
                CreateRoomPanel.transform.Find("WrongNumberText").gameObject.SetActive(true);
                return;
            }

            maxPlayers = (byte)Mathf.Clamp(maxPlayers, 2, 8);

            RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };

            PhotonNetwork.CreateRoom(roomName, options, null);
        }

        public void OnJoinRandomRoomButtonClicked()
        {
            SetActivePanel(JoinRandomRoomPanel.name);

            PhotonNetwork.JoinRandomRoom();
        }

        public void OnLeaveGameButtonClicked()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void OnLoginButtonClicked(string playerName)
        {
            PlayerNameInput.text = playerName;
            OnLoginButtonClicked();
        }

        public void OnLoginButtonClicked()
        {
            string playerName = PlayerNameInput.text;

            if (playerName.Equals(""))
            {
                LoginPanel.transform.Find("IncorrectNameLabel").gameObject.SetActive(true);
            }
            else if (LoginPanel.GetComponentInChildren<Toggle>().isOn && !AdminPasswordInput.text.Equals(adminPassword))
            {
                LoginPanel.transform.Find("IncorrectPasswordLabel").gameObject.SetActive(true);
            }
            else
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.GameVersion = gameVersion;

                adminMode = AdminPasswordInput.text.Equals(adminPassword) && LoginPanel.GetComponentInChildren<Toggle>().isOn;

                Hashtable hash = new Hashtable();
                hash.Add("admin", adminMode);
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public void OnBackToNicknameButtonClicked()
        {
            SetActivePanel(LoginPanel.name);
            LoginPanel.transform.Find("IncorrectNameLabel").gameObject.SetActive(false);
            PhotonNetwork.Disconnect();
        }

        public void OnRoomListButtonClicked()
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            SetActivePanel(RoomListPanel.name);

        }

        public void OnStartGameButtonClicked()
        {
            //PhotonNetwork.CurrentRoom.IsOpen = false;
            //PhotonNetwork.CurrentRoom.IsVisible = false;
            SetActivePanel(LoadingPanel.name);
            LoadingText.text = "Loading " + sceneToLoadString + "...";
            PhotonNetwork.LoadLevel(sceneToLoadString);
        }

        public void OnTutorialButtonClicked()
        {
            SetActivePanel(TutorialPanel.name);
        }

        public void OnStartTutorialButtonClicked()
        {
            if (TutorialPanel.GetComponentInChildren<Toggle>().isOn)
            {
                if (TutorialAdminPasswordInput.text.Equals(adminPassword))
                {
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "admin", true } });
                    SceneManager.LoadScene(tutorialToLoadString);
                }
                else
                {
                    TutorialPanel.transform.Find("IncorrectPasswordLabel").gameObject.SetActive(true);
                }
            }
            else
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "admin", false } });
                SceneManager.LoadScene(tutorialToLoadString);
            }
        }

        public void OnBackToLoginButtonClicked()
        {
            SetActivePanel(LoginPanel.name);
        }

        public void OnAdminToggleClicked(bool toggleValue)
        {
            AdminPasswordInput.gameObject.SetActive(toggleValue);
        }

        public void OnAdminTutorialToggleClicked(bool toggleValue)
        {
            TutorialAdminPasswordInput.gameObject.SetActive(toggleValue);
        }
        #endregion

        private bool CheckPlayersReady()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return false;
            }

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object isPlayerReady;
                if (p.CustomProperties.TryGetValue("IsPlayerReady", out isPlayerReady))
                {
                    if (!(bool)isPlayerReady)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void ClearRoomListView()
        {
            foreach (GameObject entry in roomListEntries.Values)
            {
                Destroy(entry.gameObject);
            }

            roomListEntries.Clear();
        }

        public void LocalPlayerPropertiesUpdated()
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        private void SetActivePanel(string activePanel)
        {
            LoginPanel.SetActive(activePanel.Equals(LoginPanel.name));
            SelectionPanel.SetActive(activePanel.Equals(SelectionPanel.name));
            CreateRoomPanel.SetActive(activePanel.Equals(CreateRoomPanel.name));
            JoinRandomRoomPanel.SetActive(activePanel.Equals(JoinRandomRoomPanel.name));
            RoomListPanel.SetActive(activePanel.Equals(RoomListPanel.name));    // UI should call OnRoomListButtonClicked() to activate this
            InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));
            TutorialPanel.SetActive(activePanel.Equals(TutorialPanel.name));
            LoadingPanel.SetActive(activePanel.Equals(LoadingPanel.name));
        }

        private void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList.Remove(info.Name);
                    }

                    continue;
                }

                // Update cached room info
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                // Add new room info to cache
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }
        }

        private void UpdateRoomListView()
        {
            foreach (RoomInfo info in cachedRoomList.Values)
            {
                GameObject entry = Instantiate(RoomListEntryPrefab);
                entry.transform.SetParent(RoomListContent.transform);
                entry.transform.localScale = Vector3.one;
                entry.GetComponent<RoomEntry>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

                roomListEntries.Add(info.Name, entry);
            }
        }

        public void Exit()
        {
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }
    }
}