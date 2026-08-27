using System;
using Fusion;
using UnityEngine;

namespace RumbleParty.Networking
{
    public interface IConnectionEvents
    {
        event Action OnConnected;
        event Action OnDisconnected;
        event Action OnConnecting;
        event Action<string> OnConnectFailed;
    }

    public interface IPlayerJoinedEvents
    {
        event Action<PlayerRef> OnPlayerJoined;
        event Action<PlayerRef> OnPlayerLeft;
    }

    public interface IPlayerSpawner
    {
        NetworkObject Spawn(NetworkRunner runner,PlayerRef player, Vector3 position, Quaternion rotation);
    }
    public interface IRoomCodeGenerator
    {
       string GenerateRoomCode();
    }
}