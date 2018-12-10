using UnityEngine;
using UnityEngine.UI;


using Photon.Pun;
using Photon.Realtime;


using System.Collections;


namespace NTNU.CarloMarton.VRLanguage
{
    /// <summary>
    /// Player name input field. Let the user input his name, will appear above the player in the game.
    /// </summary>
    [RequireComponent(typeof(InputField))]
    public class RoomNameInputField : MonoBehaviour
    {
        #region Private Constants


        // Store the PlayerPref Key to avoid typos
        const string roomNamePrefKey = "RoomName";


        #endregion

        string defaultRoom;
        private string roomName;
        InputField _inputField;

        #region MonoBehaviour CallBacks


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            defaultRoom = string.Empty;
            _inputField = this.GetComponent<InputField>();
            if (_inputField != null)
            {
                if (PlayerPrefs.HasKey(roomNamePrefKey))
                {
                    defaultRoom = PlayerPrefs.GetString(roomNamePrefKey);
                    _inputField.text = defaultRoom;
                }
            }


            roomName = defaultRoom;
        }


        #endregion


        #region Public Methods


        /// <summary>
        /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
        /// </summary>
        /// <param name="value">The name of the Player</param>
        public void SetRoomName(string value)
        {
            roomName = value;


            PlayerPrefs.SetString(roomNamePrefKey, value);
        }

        public bool ValidRoomName()
        {
            return !string.IsNullOrEmpty(roomName);
        }

        public string GetRoomName()
        {
            return roomName;
        }

        #endregion
    }
}