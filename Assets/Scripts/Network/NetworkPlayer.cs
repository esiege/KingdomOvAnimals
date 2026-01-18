using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;
using KOA.Data;

// FishNet code regeneration trigger - do not remove
// Last regenerated: 2026-01-04

namespace KOA.Network
{
    /// <summary>
    /// Represents a connected player in the network.
    /// This is spawned for each player that connects.
    /// Contains synced identity and stats (health, mana) for this player.
    /// 
    /// NOTE: Card play and ability logic has been moved to NetworkBoardState.
    /// NetworkPlayer is now identity-only plus basic stats.
    /// </summary>
    public class NetworkPlayer : NetworkBehaviour
    {
        #region Synced Identity
        
        /// <summary>
        /// The player's connection ID (synced to all clients).
        /// 0 = Player 0 (host's perspective "Player"), 1 = Player 1 (host's perspective "Opponent")
        /// </summary>
        public readonly SyncVar<int> PlayerId = new SyncVar<int>();

        /// <summary>
        /// Player's display name (synced to all clients).
        /// </summary>
        public readonly SyncVar<string> PlayerName = new SyncVar<string>();

        /// <summary>
        /// Is this player ready to start the game?
        /// </summary>
        public readonly SyncVar<bool> IsReady = new SyncVar<bool>();
        
        #endregion
        
        #region Synced Game State
        
        /// <summary>
        /// Player's current health.
        /// Initial value set to 20 so both server and client start with same expectation.
        /// </summary>
        public readonly SyncVar<int> CurrentHealth = new SyncVar<int>(20);
        
        /// <summary>
        /// Player's maximum health.
        /// </summary>
        public readonly SyncVar<int> MaxHealth = new SyncVar<int>(20);
        
        /// <summary>
        /// Player's current mana.
        /// </summary>
        public readonly SyncVar<int> CurrentMana = new SyncVar<int>(1);
        
        /// <summary>
        /// Player's maximum mana.
        /// </summary>
        public readonly SyncVar<int> MaxMana = new SyncVar<int>(1);
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Event fired when any game state changes (for UI updates).
        /// </summary>
        public System.Action OnStateChanged;
        
        #endregion

        private void Awake()
        {
            // Subscribe to sync callbacks
            PlayerId.OnChange += OnPlayerIdChanged;
            CurrentHealth.OnChange += OnHealthChanged;
            MaxHealth.OnChange += OnMaxHealthChanged;
            CurrentMana.OnChange += OnManaChanged;
            MaxMana.OnChange += OnMaxManaChanged;
        }

        private void OnDestroy()
        {
            PlayerId.OnChange -= OnPlayerIdChanged;
            CurrentHealth.OnChange -= OnHealthChanged;
            MaxHealth.OnChange -= OnMaxHealthChanged;
            CurrentMana.OnChange -= OnManaChanged;
            MaxMana.OnChange -= OnMaxManaChanged;
        }

        // Store the player ID to be used during OnStartServer
        private int _pendingPlayerId = -1;
        
        // Store pending state for reconnection (applied in OnStartServer)
        private DisconnectedPlayerState _pendingState = null;

        /// <summary>
        /// Called by server to set the player ID before spawn.
        /// Actual initialization happens in OnStartServer.
        /// </summary>
        public void SetPlayerId(int playerId)
        {
            _pendingPlayerId = playerId;
        }
        
        /// <summary>
        /// Set the state to be restored when the NetworkPlayer spawns.
        /// Must be called BEFORE spawn.
        /// </summary>
        public void SetPendingState(DisconnectedPlayerState state)
        {
            _pendingState = state;
            if (state != null)
            {
                _pendingPlayerId = state.playerId;
                Debug.Log($"[NetworkPlayer] Pending state set for player {state.playerId}: HP={state.health}, Mana={state.mana}");
            }
        }
        
        /// <summary>
        /// Restore SyncVar state from saved DisconnectedPlayerState.
        /// Called AFTER spawn so SyncVars sync to clients properly.
        /// </summary>
        [Obsolete("Use SetPendingState before spawn instead")]
        public void RestoreFromState(DisconnectedPlayerState state)
        {
            if (state == null)
            {
                Debug.LogWarning("[NetworkPlayer] RestoreFromState called with null state!");
                return;
            }
            
            Debug.Log($"[NetworkPlayer] Restoring state for player {state.playerId}: HP={state.health}/{state.maxHealth}, Mana={state.mana}/{state.maxMana}");
            
            // Restore all SyncVars - these will sync to clients since we're spawned
            PlayerId.Value = state.playerId;
            PlayerName.Value = state.playerName;
            CurrentHealth.Value = state.health;
            MaxHealth.Value = state.maxHealth;
            CurrentMana.Value = state.mana;
            MaxMana.Value = state.maxMana;
            
            Debug.Log($"[NetworkPlayer] State restored! PlayerId={PlayerId.Value}, HP={CurrentHealth.Value}, Mana={CurrentMana.Value}");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            // Check if this is a reconnection with pending state
            if (_pendingState != null)
            {
                // Reconnection - restore ALL state from pending state
                PlayerId.Value = _pendingState.playerId;
                PlayerName.Value = _pendingState.playerName;
                CurrentHealth.Value = _pendingState.health;
                MaxHealth.Value = _pendingState.maxHealth;
                CurrentMana.Value = _pendingState.mana;
                MaxMana.Value = _pendingState.maxMana;
                
                Debug.Log($"[NetworkPlayer] Server (reconnect): Restored state for player {_pendingState.playerId}: HP={_pendingState.health}/{_pendingState.maxHealth}, Mana={_pendingState.mana}/{_pendingState.maxMana}");
                
                // Clear pending state
                _pendingState = null;
                _pendingPlayerId = -1;
            }
            else if (_pendingPlayerId >= 0)
            {
                // Reconnection without full state - just use the preserved player ID
                PlayerId.Value = _pendingPlayerId;
                PlayerName.Value = $"Player {_pendingPlayerId}";
                Debug.Log($"[NetworkPlayer] Server (reconnect): PlayerId set to preserved ID={_pendingPlayerId}");
                _pendingPlayerId = -1;
            }
            else
            {
                // New player - use FishNet's Owner.ClientId as the canonical player ID
                PlayerId.Value = Owner.ClientId;
                PlayerName.Value = $"Player {Owner.ClientId}";
                Debug.Log($"[NetworkPlayer] Server: PlayerId set to Owner.ClientId={Owner.ClientId}");
            }
            
            Debug.Log($"[NetworkPlayer] Spawned: {PlayerName.Value} (ID: {PlayerId.Value}), HP={CurrentHealth.Value}, Mana={CurrentMana.Value}");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            if (IsOwner)
            {
                Debug.Log($"[NetworkPlayer] You are: {PlayerName.Value} (ID: {PlayerId.Value})");
            }
            else
            {
                Debug.Log($"[NetworkPlayer] Other player joined: {PlayerName.Value} (ID: {PlayerId.Value})");
            }
        }
        
        /// <summary>
        /// Called when ownership changes (used for reconnection).
        /// </summary>
        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            
            if (IsOwner)
            {
                Debug.Log($"[NetworkPlayer] Ownership gained! You are now: {PlayerName.Value} (ID: {PlayerId.Value})");
                
                // Re-register with NetworkGameManager (for reconnection)
                if (NetworkGameManager.Instance != null)
                {
                    NetworkGameManager.Instance.RegisterNetworkPlayer(this);
                }
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            
            if (!IsOwner)
            {
                Debug.Log($"[NetworkPlayer] Player left: {PlayerName.Value} (ID: {PlayerId.Value})");
            }
        }

        /// <summary>
        /// Called by owning client to set ready state.
        /// </summary>
        [ServerRpc]
        public void SetReady(bool ready)
        {
            IsReady.Value = ready;
            Debug.Log($"[NetworkPlayer] {PlayerName.Value} ready: {IsReady.Value}");
        }
        
        #region SyncVar Callbacks
        
        private void OnPlayerIdChanged(int prev, int next, bool asServer)
        {
            Debug.Log($"[NetworkPlayer] PlayerId changed: {prev} -> {next} (asServer: {asServer}, IsOwner: {IsOwner})");
        }
        
        private void OnHealthChanged(int prev, int next, bool asServer)
        {
            if (prev != next)
            {
                Debug.Log($"[NetworkPlayer] {PlayerName.Value} health: {prev} -> {next} (asServer: {asServer})");
            }
            OnStateChanged?.Invoke();
        }
        
        private void OnMaxHealthChanged(int prev, int next, bool asServer)
        {
            OnStateChanged?.Invoke();
        }
        
        private void OnManaChanged(int prev, int next, bool asServer)
        {
            if (prev != next)
            {
                Debug.Log($"[NetworkPlayer] {PlayerName.Value} mana: {prev} -> {next} (asServer: {asServer})");
            }
            OnStateChanged?.Invoke();
        }
        
        private void OnMaxManaChanged(int prev, int next, bool asServer)
        {
            OnStateChanged?.Invoke();
        }
        
        #endregion
        
        #region Server RPCs (Basic Stats Only)
        
        [ServerRpc(RequireOwnership = false)]
        public void CmdTakeDamage(int amount)
        {
            if (amount <= 0) return;
            
            int newHealth = Mathf.Max(0, CurrentHealth.Value - amount);
            CurrentHealth.Value = newHealth;
            
            Debug.Log($"[Server] {PlayerName.Value} took {amount} damage. Health: {newHealth}");
            
            if (newHealth <= 0)
            {
                Debug.Log($"[Server] {PlayerName.Value} has died!");
                // Game over logic handled by NetworkBoardState
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void CmdHeal(int amount)
        {
            if (amount <= 0) return;
            
            int newHealth = Mathf.Min(MaxHealth.Value, CurrentHealth.Value + amount);
            CurrentHealth.Value = newHealth;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void CmdSpendMana(int amount)
        {
            if (amount <= 0) return;
            
            if (CurrentMana.Value >= amount)
            {
                CurrentMana.Value -= amount;
                Debug.Log($"[Server] {PlayerName.Value} spent {amount} mana. Remaining: {CurrentMana.Value}");
            }
            else
            {
                Debug.LogWarning($"[Server] {PlayerName.Value} doesn't have enough mana! Has {CurrentMana.Value}, needs {amount}");
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void CmdRefillMana()
        {
            CurrentMana.Value = MaxMana.Value;
            Debug.Log($"[Server] {PlayerName.Value} mana refilled to {MaxMana.Value}");
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void CmdIncreaseMaxMana(int amount)
        {
            Debug.Log($"[Server] CmdIncreaseMaxMana received for {PlayerName.Value}, amount: {amount}");
            if (amount <= 0) return;
            
            MaxMana.Value += amount;
            CurrentMana.Value = MaxMana.Value;
            Debug.Log($"[Server] {PlayerName.Value} max mana increased to {MaxMana.Value}");
        }
        
        #endregion
        
        #region Reconnection Support
        
        /// <summary>
        /// Client sends saved game state to host for restoration after host reconnects.
        /// </summary>
        // NOTE: GameStateSnapshot removed in Story 036
        // Reconnection now uses NetworkBoardState SyncVar to restore state
        [ServerRpc]
        public void ServerRestoreGameState(string gameStateJson)
        {
            Debug.Log($"[Server] ServerRestoreGameState called but GameStateSnapshot is deprecated. State is restored via NetworkBoardState SyncVar.");
            // No-op: NetworkBoardState SyncVar handles state restoration on reconnect
        }
        
        #endregion
        
        #region Helper Methods
        
        public bool HasEnoughMana(int cost) => CurrentMana.Value >= cost;
        public bool IsAlive => CurrentHealth.Value > 0;
        
        #endregion
    }
}
