using Fusion;

namespace GameNetworking.Player
{
   
    public class PlayerEntry : NetworkBehaviour
    {
        [Networked, Capacity(24)] private string _nickname { get; set; } = "Player";

        [Networked] public NetworkBool IsReady { get; set; }
        [Networked] public PlayerRef Owner { get; set; }

        public string Nickname => _nickname;

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
                RPC_SetNickname(GameNetworking.Networking.ConnectionManager.Instance.LocalNickname);

            PlayerRegistry.Instance?.Register(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
            => PlayerRegistry.Instance?.Unregister(this);

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SetNickname(string nickname)
            => _nickname = string.IsNullOrWhiteSpace(nickname) ? "Player" : nickname;

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SetReady(NetworkBool ready)
        {
            IsReady = ready;
            PlayerRegistry.Instance?.NotifyChanged();
        }
    }
}