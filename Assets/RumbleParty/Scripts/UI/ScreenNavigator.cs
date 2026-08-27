using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameNetworking.UI
{
    public enum ScreenId { Home, LobbySelection, RoomLobby }
    
    public class ScreenNavigator : MonoBehaviour
    {
        public static ScreenNavigator Instance { get; private set; }

        [Serializable]
        public struct ScreenEntry
        {
            public ScreenId Id;
            public GameObject Panel;
        }

        [SerializeField] private List<ScreenEntry> _screens = new List<ScreenEntry>();
        [SerializeField] private ScreenId _initialScreen = ScreenId.Home;

        private readonly Dictionary<ScreenId, GameObject> _lookup = new Dictionary<ScreenId, GameObject>();

        void Awake()
        {
            Instance = this;
            foreach (var entry in _screens) _lookup[entry.Id] = entry.Panel;
            Show(_initialScreen);
        }

        public void Show(ScreenId screen)
        {
            foreach (var kvp in _lookup)
                kvp.Value.SetActive(kvp.Key == screen);
        }
    }
}