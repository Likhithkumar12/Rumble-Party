using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameNetworking.Player;

namespace GameNetworking.Networking
{
    /// <summary>
    /// Owns the NetworkRunner and the connect/create/join/start lifecycle.
    /// Persistent singleton created in the Menu scene. Every UI script talks
    /// to this directly - simple, one hop, easy to debug.
    ///
    /// Two scenes total: Menu (Home/LobbySelection/RoomLobby are panels
    /// inside it, swapped by ScreenNavigator) and Gameplay. The ONLY
    /// Runner.LoadScene call in the project is StartGameplay().
    /// </summary>
    [RequireComponent(typeof(NetworkRunner))]
    public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static ConnectionManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private NetworkObject _playerRegistryPrefab;
        [SerializeField] private NetworkObject _playerEntryPrefab;

        [Header("Config")]
        [SerializeField] private string _gameplaySceneName = "Gameplay";

        public NetworkRunner Runner { get; private set; }
        public string CurrentRoomCode { get; private set; }
        public bool IsHost => Runner != null && Runner.IsServer;
        public string LocalNickname { get; set; } = "Player";

        public event Action Connected;
        public event Action<string> ConnectFailed;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Runner = GetComponent<NetworkRunner>();
            Runner.ProvideInput = true;
            Runner.AddCallbacks(this);
        }

        public async Task<bool> CreateRoomAsync()
        {
            string code = GenerateRoomCode();
            bool ok = await StartGameInternal(GameMode.Host, code);
            if (ok) CurrentRoomCode = code;
            return ok;
        }

        public async Task<bool> JoinRoomAsync(string roomCode)
        {
            string normalized = roomCode.Trim().ToUpperInvariant();
            bool ok = await StartGameInternal(GameMode.Client, normalized);
            if (ok) CurrentRoomCode = normalized;
            return ok;
        }

        /// <summary>Host-only. The one and only scene load in the project.</summary>
        public void StartGameplay()
        {
            if (!Runner.IsServer)
            {
                Debug.LogWarning("Only the host can start the game.");
                return;
            }

            int buildIndex = SceneUtility.GetBuildIndexByScenePath(GetScenePathByName(_gameplaySceneName));
            Runner.LoadScene(SceneRef.FromIndex(buildIndex), LoadSceneMode.Single);
        }

        private async Task<bool> StartGameInternal(GameMode mode, string sessionName)
        {
            var sceneManager = GetComponent<NetworkSceneManagerDefault>()
                             ?? gameObject.AddComponent<NetworkSceneManagerDefault>();

            var result = await Runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = sessionName,
                PlayerCount = 8,
                SceneManager = sceneManager,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex)
            });

            if (result.Ok)
            {
                Connected?.Invoke();
                return true;
            }

            ConnectFailed?.Invoke(result.ShutdownReason.ToString());
            return false;
        }

        private static string GenerateRoomCode(int length = 5)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O, 1/I
            var sb = new StringBuilder(length);
            var rnd = new System.Random();
            for (int i = 0; i < length; i++) sb.Append(alphabet[rnd.Next(alphabet.Length)]);
            return sb.ToString();
        }

        private static string GetScenePathByName(string sceneName)
        {
            int count = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < count; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                if (path.EndsWith(sceneName + ".unity")) return path;
            }
            Debug.LogError($"Scene '{sceneName}' not found in Build Settings.");
            return string.Empty;
        }

        // ---- INetworkRunnerCallbacks ----

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                if (FindFirstObjectByType<PlayerRegistry>() == null)
                {
                    Debug.Log("Spawning PlayerRegistery");
                    Runner.Spawn(_playerRegistryPrefab, Vector3.zero, Quaternion.identity);
                }
                var entryObj = runner.Spawn(_playerEntryPrefab, inputAuthority: player);
                entryObj.GetComponent<PlayerEntry>().Owner = player;
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason reason) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
            => ConnectFailed?.Invoke(reason.ToString());

        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data)
        {
            
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
    }
}