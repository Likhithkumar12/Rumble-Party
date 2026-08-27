using System;
using GameNetworking.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameNetworking.UI
{
    public class LobbySelectionUI:MonoBehaviour
    {
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _joinRoomButton;
        [SerializeField] private TMP_InputField _roomCodeField;
        [SerializeField] private TextMeshProUGUI _statusText;

        void Start()
        {
            _createRoomButton.onClick.AddListener(onCreateRoomClicked);
            _joinRoomButton.onClick.AddListener(onJoinRoomClicked);
        }

        private void OnEnable()
        {
            ResetState();
        }

        private async void onCreateRoomClicked()
        {
            SetButtonsInteractable(false);
            _statusText.text = "Creating room...";
 
            bool ok = await ConnectionManager.Instance.CreateRoomAsync();
 
            if (ok) ScreenNavigator.Instance.Show(ScreenId.RoomLobby);
            else
            {
                _statusText.text = "Failed to create room.";
                SetButtonsInteractable(true);
            }

        }

        private async void onJoinRoomClicked()
        {
            string code = _roomCodeField.text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                _statusText.text = "Enter a room code.";
                return;
            }
 
            SetButtonsInteractable(false);
            _statusText.text = "Joining room...";
 
            bool ok = await ConnectionManager.Instance.JoinRoomAsync(code);
 
            if (ok) ScreenNavigator.Instance.Show(ScreenId.RoomLobby);
            else
            {
                _statusText.text = "Room not found or full.";
                SetButtonsInteractable(true);
            }
        }
        private void SetButtonsInteractable(bool value)
        {
            _createRoomButton.interactable = value;
            _joinRoomButton.interactable = value;
        }

        private void ResetState()
        {
            _statusText.text = "";
            _roomCodeField.text = "";
            SetButtonsInteractable(true);
        }
        
    }
}