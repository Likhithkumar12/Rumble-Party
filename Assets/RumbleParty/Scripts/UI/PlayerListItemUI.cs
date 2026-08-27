using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameNetworking.Player;

namespace GameNetworking.UI
{
    public class PlayerListItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Image _readyIndicator;
        [SerializeField] private Color _readyColor = Color.green;
        [SerializeField] private Color _notReadyColor = Color.gray;

        public void Bind(PlayerEntry entry, bool isLocal)
        {
            _nameText.text = isLocal ? $"{entry.Nickname} (you)" : entry.Nickname;
            _readyIndicator.color = entry.IsReady ? _readyColor : _notReadyColor;
        }
    }
}