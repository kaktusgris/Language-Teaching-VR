using ExitGames.Client.Photon;
using NTNU.CarloMarton.VRLanguage;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class LoginPanel : MonoBehaviourPunCallbacks
{

    public MainPanel mainPanel;

    private bool bypass = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L) && Input.GetKeyDown(KeyCode.I))
        {
            BypassLogin();
        }
    }

    private void BypassLogin()
    {
        print("Bypass");
        bypass = true;
        PhotonNetwork.LocalPlayer.NickName = "test_name";
        PhotonNetwork.GameVersion = "0";
        PhotonNetwork.ConnectUsingSettings();

    }

    public override void OnConnectedToMaster()
    {
        if (bypass)
        {
            Hashtable hash = new Hashtable();
            hash.Add("admin", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

            PhotonNetwork.CreateRoom("test" + Random.value.ToString(), new Photon.Realtime.RoomOptions { MaxPlayers = 1 }, null);
            PhotonNetwork.LoadLevel(mainPanel.sceneToLoadString);
        }
        else
        {
            mainPanel.OnConnectedToMaster();
        }
    }
}
