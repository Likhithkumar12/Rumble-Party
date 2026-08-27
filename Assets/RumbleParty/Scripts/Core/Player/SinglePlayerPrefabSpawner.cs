using Fusion;
using RumbleParty.Networking;
using UnityEngine;

namespace GameNetworking.Networking
{
    public class SinglePrefabPlayerSpawner : MonoBehaviour, IPlayerSpawner
    {
        [SerializeField] private NetworkObject _characterPrefab;

        public NetworkObject Spawn(NetworkRunner runner, PlayerRef player, Vector3 position, Quaternion rotation)
            => runner.Spawn(_characterPrefab, position, rotation, player);
    }
}