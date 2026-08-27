using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace GameNetworking.Player
{
    public class PlayerRegistry : NetworkBehaviour
    {
        public static PlayerRegistry Instance { get; private set; }

        public event Action OnRosterChanged;

        public static event Action<PlayerRegistry> onReady;

        private readonly List<PlayerEntry> _entries = new List<PlayerEntry>();
        public IReadOnlyList<PlayerEntry> Entries => _entries;

        public bool AllReady => _entries.Count > 0 && _entries.All(e => e.IsReady);
        public int ReadyCount => _entries.Count(e => e.IsReady);
        public int PlayerCount => _entries.Count;

        public override void Spawned()
        {
            Instance = this;
            onReady?.Invoke(this);
        }

        public void Register(PlayerEntry entry)
        {
            if (_entries.Contains(entry)) return;
            _entries.Add(entry);
            NotifyChanged();
        }

        public void Unregister(PlayerEntry entry)
        {
            if (_entries.Remove(entry)) NotifyChanged();
        }
        

        public void NotifyChanged() => OnRosterChanged?.Invoke();

        public void SetLocalReady(bool ready, PlayerRef localPlayer)
            => _entries.FirstOrDefault(e => e.Owner == localPlayer)?.RPC_SetReady(ready);
    }
}