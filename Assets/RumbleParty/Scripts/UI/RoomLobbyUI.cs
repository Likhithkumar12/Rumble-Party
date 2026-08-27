using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameNetworking.Player;
using GameNetworking.Networking;

namespace GameNetworking.UI
{
    public class RoomLobbyUI : MonoBehaviour
    {
        [SerializeField] private Transform _playerListContainer;
        [SerializeField] private PlayerListItemUI _playerListItemPrefab;
        [SerializeField] private TMP_Text _roomCodeText;
        [SerializeField] private TMP_Text _readyCountText;
        [SerializeField] private Button _readyButton;
        [SerializeField] private TMP_Text _readyButtonLabel;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _backButton;

        private bool _localReady;

        void OnEnable()
        {
            _roomCodeText.text = ConnectionManager.Instance.CurrentRoomCode;
            _readyButton.onClick.AddListener(OnReadyClicked);
            _startButton.onClick.AddListener(OnStartClicked);
            _startButton.gameObject.SetActive(ConnectionManager.Instance.IsHost);
            _backButton.onClick.AddListener(OnBackClicked);

            if (PlayerRegistry.Instance != null)
                PlayerRegistry.Instance.OnRosterChanged += Refresh;
            if (PlayerRegistry.Instance != null)
            {
              
                BindToRegistry(PlayerRegistry.Instance);
            }
            else
            {
               
                PlayerRegistry.onReady += BindToRegistry;
            }

        }

        void OnDisable()
        {
            if (PlayerRegistry.Instance != null)
                PlayerRegistry.Instance.OnRosterChanged -= Refresh;
            if (PlayerRegistry.Instance != null)
                PlayerRegistry.Instance.OnRosterChanged -= Refresh;
            PlayerRegistry.onReady -= BindToRegistry;
            _backButton.onClick.RemoveListener(OnBackClicked);
        }
        private void OnBackClicked()
        {
            if (ConnectionManager.Instance.IsHost)
            {
                ConnectionManager.Instance.ShutdownRoom();
            }
            else
            {
                ConnectionManager.Instance.LeaveRoom(ConnectionManager.Instance.Runner.LocalPlayer);
            }

            ScreenNavigator.Instance.Show(ScreenId.Home);
        }

        private void OnReadyClicked()
        {
            _localReady = !_localReady;
            PlayerRegistry.Instance.SetLocalReady(_localReady, ConnectionManager.Instance.Runner.LocalPlayer);
            _readyButtonLabel.text = _localReady ? "Not Ready" : "Ready";
        }

        private void OnStartClicked()
        {
            if (!ConnectionManager.Instance.IsHost || !PlayerRegistry.Instance.AllReady) return;
            ConnectionManager.Instance.StartGameplay();
        }
        private void BindToRegistry(PlayerRegistry registry)
        {
            registry.OnRosterChanged += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            foreach(Transform child in _playerListContainer)
            {
                Destroy(child.gameObject);
            }
            var registry = PlayerRegistry.Instance;
            if (registry == null)
            {
                Debug.Log("registry not found");
                return;
            }

            var local = ConnectionManager.Instance.Runner.LocalPlayer;
            Debug.Log("Register entries count: " + registry.PlayerCount);
            foreach (var entry in registry.Entries)
            {
                var item = Instantiate(_playerListItemPrefab, _playerListContainer);
                item.Bind(entry, entry.Owner == local);
            }

            _readyCountText.text = $"{registry.ReadyCount}/{registry.PlayerCount} ready";
            _startButton.interactable = registry.AllReady;
        }
    }
}