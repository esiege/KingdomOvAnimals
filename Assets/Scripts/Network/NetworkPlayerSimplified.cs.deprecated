using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace KOA.Network
{
    /// <summary>
    /// Simplified NetworkPlayer - identity and connection only.
    /// Game state (health, mana, cards) is now managed by NetworkBoardState.
    /// This class exists solely for:
    /// 1. Player identity (PlayerId, PlayerName)
    /// 2. Connection ownership (IsOwner)
    /// 3. Ready state for lobby
    /// </summary>
    public class NetworkPlayerSimplified : NetworkBehaviour
    {
        #region Synced Identity
        
        /// <summary>
        /// The player's ID (0 or 1). Matches BoardState.Players[PlayerId].
        /// </summary>
        public readonly SyncVar<int> PlayerId = new SyncVar<int>(-1);

        /// <summary>
        /// Player's display name.
        /// </summary>
        public readonly SyncVar<string> PlayerName = new SyncVar<string>("");

        /// <summary>
        /// Is this player ready to start the game?
        /// </summary>
        public readonly SyncVar<bool> IsReady = new SyncVar<bool>(false);
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Fired when player identity changes.
        /// </summary>
        public System.Action<int> OnPlayerIdChanged;
        
        #endregion

        private void Awake()
        {
            PlayerId.OnChange += HandlePlayerIdChanged;
        }

        private void OnDestroy()
        {
            PlayerId.OnChange -= HandlePlayerIdChanged;
        }
        
        private void HandlePlayerIdChanged(int prev, int next, bool asServer)
        {
            Debug.Log($"[NetworkPlayerSimplified] PlayerId changed: {prev} -> {next}");
            OnPlayerIdChanged?.Invoke(next);
        }

        // Pending ID for reconnection
        private int _pendingPlayerId = -1;

        /// <summary>
        /// Set player ID before spawn (for reconnection).
        /// </summary>
        public void SetPlayerId(int playerId)
        {
            _pendingPlayerId = playerId;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            if (_pendingPlayerId >= 0)
            {
                // Reconnection - use preserved ID
                PlayerId.Value = _pendingPlayerId;
                PlayerName.Value = $"Player {_pendingPlayerId}";
                Debug.Log($"[NetworkPlayerSimplified] Server (reconnect): PlayerId={_pendingPlayerId}");
                _pendingPlayerId = -1;
            }
            else
            {
                // New connection - assign based on connection count
                // NetworkGameManager will assign the actual player ID
                PlayerId.Value = Owner.ClientId;
                PlayerName.Value = $"Player {Owner.ClientId}";
                Debug.Log($"[NetworkPlayerSimplified] Server: New player ClientId={Owner.ClientId}");
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            if (IsOwner)
            {
                Debug.Log($"[NetworkPlayerSimplified] You are: {PlayerName.Value} (ID: {PlayerId.Value})");
            }
        }

        /// <summary>
        /// Set ready state.
        /// </summary>
        [ServerRpc]
        public void CmdSetReady(bool ready)
        {
            IsReady.Value = ready;
            Debug.Log($"[NetworkPlayerSimplified] {PlayerName.Value} ready: {ready}");
        }
    }
}
