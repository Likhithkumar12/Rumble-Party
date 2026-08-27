using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameNetworking.Networking;

namespace GameNetworking.UI
{
    public class HomeScreenUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nicknameField;
        [SerializeField] private Button _playButton;

        private const string NicknamePrefKey = "player_nickname";

        void Start()
        {
            _nicknameField.text = PlayerPrefs.GetString(NicknamePrefKey, "Player" + Random.Range(100, 999));
            _playButton.onClick.AddListener(OnPlayClicked);
        }

        private void OnPlayClicked()
        {
            string nickname = string.IsNullOrWhiteSpace(_nicknameField.text)
                ? "Player" : _nicknameField.text.Trim();

            PlayerPrefs.SetString(NicknamePrefKey, nickname);
            ConnectionManager.Instance.LocalNickname = nickname;

            ScreenNavigator.Instance.Show(ScreenId.LobbySelection);
        }
    }
}