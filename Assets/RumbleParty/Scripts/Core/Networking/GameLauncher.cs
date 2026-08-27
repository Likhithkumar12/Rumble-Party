

using Fusion;
using GameNetworking.Networking;
using GameNetworking.Player;
using UnityEngine;

namespace RumbleParty.Networking
{
    public class GameLauncher:MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _spawnerBehaviour; 
        private IPlayerSpawner _spawner;

        private void Awake()
        {
            _spawner=_spawnerBehaviour as IPlayerSpawner;
            if (_spawner == null)
            {
                Debug.LogError("GameLauncher:Awake(): Spawner must be assigned");
            }
        }

        public  void TryStartGame()
        {
            var runner = ConnectionManager.Instance.Runner;
            if (!runner.IsServer)
            {
                Debug.LogWarning("Only the host can start the game.");
                return;
            }
 
            if (!PlayerRegistry.Instance.AllReady)
            {
                Debug.LogWarning("Not all players are ready.");
                return;
            }
 
           // ConnectionManager.Instance.LoadGameplayScene();  
        }
        public NetworkObject SpawnCharacterFor(PlayerRef player, Vector3 position, Quaternion rotation)
        {
            return _spawner.Spawn(ConnectionManager.Instance.Runner, player, position, rotation);
        }
        
    }
}