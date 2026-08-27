

using Fusion;
using RumbleParty.Networking;
using UnityEngine;

namespace RumbleParty.GamePlay
{
    public class Gameplayscenebootstrap:NetworkBehaviour
    {
        [SerializeField] private GameLauncher _launcher;
        [SerializeField] private Transform[] _spawnPoints;
        
        private int _nextSpawnIndex;
 
        public override void Spawned()
        {
            if (!Object.HasStateAuthority) return;
 
            foreach (var player in Runner.ActivePlayers)
                SpawnFor(player);
        }
        
        public void OnPlayerJoinedLate(PlayerRef player)
        {
            if (Object.HasStateAuthority) SpawnFor(player);
        }
 
        private void SpawnFor(PlayerRef player)
        {
            Transform point = _spawnPoints.Length > 0
                ? _spawnPoints[_nextSpawnIndex++ % _spawnPoints.Length]
                : transform;
 
            _launcher.SpawnCharacterFor(player, point.position, point.rotation);
        }
        
    }
}