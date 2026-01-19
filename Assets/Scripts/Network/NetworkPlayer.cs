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
        #region Build Stamp
        
        // BUILD STAMP - Change this every time we rebuild to confirm which code is running
        // Format: YYYYMMDD_HHMM
        public const string BUILD_STAMP = "20260119_0004";
        
        #endregion
        
        #region Synced Identity
        
        /// <summary>
        /// The player's connection ID (synced to all clients).
        /// 0 = Player 0 (host's perspective "Player"), 1 = Player 1 (host's perspective "Opponent")
        /// Initial value is -1 (unset) so the actual value is always sent on spawn.
        /// </summary>
        public readonly SyncVar<int> PlayerId = new SyncVar<int>(-1);

        /// <summary>
        /// Player's display name (synced to all clients).
        /// Initial value is sentinel so actual name is always sent.
        /// </summary>
        public readonly SyncVar<string> PlayerName = new SyncVar<string>("__UNSET__");

        /// <summary>
        /// Is this player ready to start the game?
        /// Stored as int: -1 = unset (sentinel), 0 = not ready, 1 = ready.
        /// This is int instead of bool because FishNet's WriteFull skips SyncVars where
        /// value equals initial value, causing serialization count mismatches.
        /// </summary>
        public readonly SyncVar<int> IsReady = new SyncVar<int>(-1);
        
        #endregion
        
        #region Synced Game State
        
        /// <summary>
        /// Player's current health.
        /// Initial value is -1 (unset) so the actual value is always sent on spawn.
        /// </summary>
        public readonly SyncVar<int> CurrentHealth = new SyncVar<int>(-1);
        
        /// <summary>
        /// Player's maximum health.
        /// Initial value is -1 (unset) so the actual value is always sent on spawn.
        /// </summary>
        public readonly SyncVar<int> MaxHealth = new SyncVar<int>(-1);
        
        /// <summary>
        /// Player's current mana.
        /// Initial value is -1 (unset) so the actual value is always sent on spawn.
        /// </summary>
        public readonly SyncVar<int> CurrentMana = new SyncVar<int>(-1);
        
        /// <summary>
        /// Player's maximum mana.
        /// Initial value is -1 (unset) so the actual value is always sent on spawn.
        /// </summary>
        public readonly SyncVar<int> MaxMana = new SyncVar<int>(-1);
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Event fired when any game state changes (for UI updates).
        /// </summary>
        public System.Action OnStateChanged;
        
        #endregion

        private void Awake()
        {
            string env = Application.isEditor ? "EDITOR" : "BUILD";
            Debug.Log($"[NetworkPlayer] ========== AWAKE ==========");
            Debug.Log($"[NetworkPlayer] BUILD_STAMP: {BUILD_STAMP}");
            Debug.Log($"[NetworkPlayer] Environment: {env}");
            Debug.Log($"[NetworkPlayer] Instance: {GetInstanceID()}");
            Debug.Log($"[NetworkPlayer] GameObject: {gameObject.name}");
            
            // Log COMPLETE SyncVar inventory with indices and initial values
            // This is the authoritative list - do not assume counts!
            Debug.Log($"[NetworkPlayer] === SYNCVAR INVENTORY (Awake) ===");
            Debug.Log($"[NetworkPlayer] SyncVar #1: PlayerId");
            Debug.Log($"[NetworkPlayer]   - SyncIndex: {PlayerId.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   - InitialValue: {PlayerId.Value}");
            Debug.Log($"[NetworkPlayer]   - Type: SyncVar<int>");
            
            Debug.Log($"[NetworkPlayer] SyncVar #2: PlayerName");
            Debug.Log($"[NetworkPlayer]   - SyncIndex: {PlayerName.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   - InitialValue: '{PlayerName.Value}'");
            Debug.Log($"[NetworkPlayer]   - Type: SyncVar<string>");
            
            Debug.Log($"[NetworkPlayer] SyncVar #3: IsReady");
            Debug.Log($"[NetworkPlayer]   - SyncIndex: {IsReady.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   - InitialValue: {IsReady.Value}");
            Debug.Log($"[NetworkPlayer]   - Type: SyncVar<int> (sentinel: -1=unset, 0=false, 1=true)");
            
            Debug.Log($"[NetworkPlayer] SyncVar #4: CurrentHealth");
            Debug.Log($"[NetworkPlayer]   - SyncIndex: {CurrentHealth.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   - InitialValue: {CurrentHealth.Value}");
            Debug.Log($"[NetworkPlayer]   - Type: SyncVar<int>");
            
            Debug.Log($"[NetworkPlayer] SyncVar #5: MaxHealth");
            Debug.Log($"[NetworkPlayer]   - SyncIndex: {MaxHealth.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   - InitialValue: {MaxHealth.Value}");
            Debug.Log($"[NetworkPlayer]   - Type: SyncVar<int>");
            
            Debug.Log($"[NetworkPlayer] SyncVar #6: CurrentMana");
            Debug.Log($"[NetworkPlayer]   - SyncIndex: {CurrentMana.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   - InitialValue: {CurrentMana.Value}");
            Debug.Log($"[NetworkPlayer]   - Type: SyncVar<int>");
            
            Debug.Log($"[NetworkPlayer] SyncVar #7: MaxMana");
            Debug.Log($"[NetworkPlayer]   - SyncIndex: {MaxMana.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   - InitialValue: {MaxMana.Value}");
            Debug.Log($"[NetworkPlayer]   - Type: SyncVar<int>");
            
            Debug.Log($"[NetworkPlayer] === TOTAL SYNCVARS: 7 (indices should be 0-6) ===");
            
            // Subscribe to sync callbacks
            PlayerId.OnChange += OnPlayerIdChanged;
            PlayerName.OnChange += OnPlayerNameChanged;
            IsReady.OnChange += OnIsReadyChanged;
            CurrentHealth.OnChange += OnHealthChanged;
            MaxHealth.OnChange += OnMaxHealthChanged;
            CurrentMana.OnChange += OnManaChanged;
            MaxMana.OnChange += OnMaxManaChanged;
            
            Debug.Log($"[NetworkPlayer] Subscribed to OnChange callbacks for all 7 SyncVars");
            Debug.Log($"[NetworkPlayer] ========== AWAKE COMPLETE ==========");
        }

        private void OnDestroy()
        {
            Debug.Log($"[NetworkPlayer] OnDestroy: {gameObject.name}, Instance: {GetInstanceID()}");
            PlayerId.OnChange -= OnPlayerIdChanged;
            PlayerName.OnChange -= OnPlayerNameChanged;
            IsReady.OnChange -= OnIsReadyChanged;
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
            
            Debug.Log($"[NetworkPlayer] ========== OnStartServer ==========");
            Debug.Log($"[NetworkPlayer] BUILD_STAMP: {BUILD_STAMP}");
            Debug.Log($"[NetworkPlayer] ObjectId: {ObjectId}");
            Debug.Log($"[NetworkPlayer] Owner.ClientId: {Owner?.ClientId ?? -999}");
            Debug.Log($"[NetworkPlayer] _pendingPlayerId: {_pendingPlayerId}");
            Debug.Log($"[NetworkPlayer] _pendingState: {(_pendingState != null ? "SET" : "NULL")}");
            
            // CRITICAL: Dump SyncVar indices to verify they match between build and editor
            Debug.Log($"[NetworkPlayer] SyncVar SyncIndex values (from IL weaver):");
            Debug.Log($"[NetworkPlayer]   PlayerId.SyncIndex = {PlayerId.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   PlayerName.SyncIndex = {PlayerName.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   IsReady.SyncIndex = {IsReady.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   CurrentHealth.SyncIndex = {CurrentHealth.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   MaxHealth.SyncIndex = {MaxHealth.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   CurrentMana.SyncIndex = {CurrentMana.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   MaxMana.SyncIndex = {MaxMana.SyncIndex}");
            
            Debug.Log($"[NetworkPlayer] SyncVar values BEFORE assignment:");
            Debug.Log($"[NetworkPlayer]   PlayerId.Value = {PlayerId.Value}");
            Debug.Log($"[NetworkPlayer]   PlayerName.Value = '{PlayerName.Value}'");
            Debug.Log($"[NetworkPlayer]   IsReady.Value = {IsReady.Value}");
            Debug.Log($"[NetworkPlayer]   CurrentHealth.Value = {CurrentHealth.Value}");
            Debug.Log($"[NetworkPlayer]   MaxHealth.Value = {MaxHealth.Value}");
            Debug.Log($"[NetworkPlayer]   CurrentMana.Value = {CurrentMana.Value}");
            Debug.Log($"[NetworkPlayer]   MaxMana.Value = {MaxMana.Value}");
            
            // Check if this is a reconnection with pending state
            if (_pendingState != null)
            {
                Debug.Log($"[NetworkPlayer] === RECONNECT PATH (pending state) ===");
                // Reconnection - restore ALL state from pending state
                PlayerId.Value = _pendingState.playerId;
                PlayerName.Value = _pendingState.playerName;
                IsReady.Value = 0; // Reset to not ready on reconnect (ensure differs from sentinel -1)
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
                Debug.Log($"[NetworkPlayer] === RECONNECT PATH (pending ID only) ===");
                // Reconnection without full state - just use the preserved player ID
                PlayerId.Value = _pendingPlayerId;
                PlayerName.Value = $"Player {_pendingPlayerId}";
                // Set default game state values
                IsReady.Value = 0; // Not ready (ensure differs from sentinel -1)
                CurrentHealth.Value = 20;
                MaxHealth.Value = 20;
                CurrentMana.Value = 1;
                MaxMana.Value = 1;
                Debug.Log($"[NetworkPlayer] Server (reconnect): PlayerId set to preserved ID={_pendingPlayerId}");
                _pendingPlayerId = -1;
            }
            else
            {
                Debug.Log($"[NetworkPlayer] === NEW PLAYER PATH ===");
                // New player - use FishNet's Owner.ClientId as the canonical player ID
                PlayerId.Value = Owner.ClientId;
                PlayerName.Value = $"Player {Owner.ClientId}";
                // Set default game state values
                IsReady.Value = 0; // Not ready (ensure it differs from sentinel -1)
                CurrentHealth.Value = 20;
                MaxHealth.Value = 20;
                CurrentMana.Value = 1;
                MaxMana.Value = 1;
                Debug.Log($"[NetworkPlayer] Server: PlayerId set to Owner.ClientId={Owner.ClientId}");
            }
            
            Debug.Log($"[NetworkPlayer] SyncVar values AFTER assignment:");
            Debug.Log($"[NetworkPlayer]   PlayerId.Value = {PlayerId.Value}");
            Debug.Log($"[NetworkPlayer]   PlayerName.Value = '{PlayerName.Value}'");
            Debug.Log($"[NetworkPlayer]   IsReady.Value = {IsReady.Value}");
            Debug.Log($"[NetworkPlayer]   CurrentHealth.Value = {CurrentHealth.Value}");
            Debug.Log($"[NetworkPlayer]   MaxHealth.Value = {MaxHealth.Value}");
            Debug.Log($"[NetworkPlayer]   CurrentMana.Value = {CurrentMana.Value}");
            Debug.Log($"[NetworkPlayer]   MaxMana.Value = {MaxMana.Value}");
            
            Debug.Log($"[NetworkPlayer] ========== OnStartServer COMPLETE ==========");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            Debug.Log($"[NetworkPlayer] ========== OnStartClient ==========");
            Debug.Log($"[NetworkPlayer] BUILD_STAMP: {BUILD_STAMP}");
            Debug.Log($"[NetworkPlayer] ObjectId: {ObjectId}");
            Debug.Log($"[NetworkPlayer] IsOwner: {IsOwner}");
            Debug.Log($"[NetworkPlayer] IsServer: {IsServer}");
            Debug.Log($"[NetworkPlayer] IsHost: {IsHost}");
            
            // CRITICAL: Dump SyncVar indices to verify they match between build and editor
            Debug.Log($"[NetworkPlayer] SyncVar SyncIndex values (from IL weaver):");
            Debug.Log($"[NetworkPlayer]   PlayerId.SyncIndex = {PlayerId.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   PlayerName.SyncIndex = {PlayerName.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   IsReady.SyncIndex = {IsReady.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   CurrentHealth.SyncIndex = {CurrentHealth.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   MaxHealth.SyncIndex = {MaxHealth.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   CurrentMana.SyncIndex = {CurrentMana.SyncIndex}");
            Debug.Log($"[NetworkPlayer]   MaxMana.SyncIndex = {MaxMana.SyncIndex}");
            
            Debug.Log($"[NetworkPlayer] SyncVar values received from server:");
            Debug.Log($"[NetworkPlayer]   PlayerId.Value = {PlayerId.Value}");
            Debug.Log($"[NetworkPlayer]   PlayerName.Value = '{PlayerName.Value}'");
            Debug.Log($"[NetworkPlayer]   IsReady.Value = {IsReady.Value}");
            Debug.Log($"[NetworkPlayer]   CurrentHealth.Value = {CurrentHealth.Value}");
            Debug.Log($"[NetworkPlayer]   MaxHealth.Value = {MaxHealth.Value}");
            Debug.Log($"[NetworkPlayer]   CurrentMana.Value = {CurrentMana.Value}");
            Debug.Log($"[NetworkPlayer]   MaxMana.Value = {MaxMana.Value}");
            
            if (IsOwner)
            {
                Debug.Log($"[NetworkPlayer] YOU ARE THIS PLAYER: {PlayerName.Value} (ID: {PlayerId.Value})");
            }
            else
            {
                Debug.Log($"[NetworkPlayer] OTHER PLAYER: {PlayerName.Value} (ID: {PlayerId.Value})");
            }
            Debug.Log($"[NetworkPlayer] ========== OnStartClient COMPLETE ==========");
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
            IsReady.Value = ready ? 1 : 0;
            Debug.Log($"[NetworkPlayer] {PlayerName.Value} ready: {IsReady.Value == 1}");
        }
        
        #region SyncVar Callbacks
        
        private void OnPlayerIdChanged(int prev, int next, bool asServer)
        {
            Debug.Log($"[NetworkPlayer] SYNCVAR CHANGE: PlayerId {prev} -> {next} (asServer: {asServer}, IsOwner: {IsOwner}, ObjectId: {ObjectId})");
        }
        
        private void OnPlayerNameChanged(string prev, string next, bool asServer)
        {
            Debug.Log($"[NetworkPlayer] SYNCVAR CHANGE: PlayerName '{prev}' -> '{next}' (asServer: {asServer}, IsOwner: {IsOwner}, ObjectId: {ObjectId})");
        }
        
        private void OnIsReadyChanged(int prev, int next, bool asServer)
        {
            Debug.Log($"[NetworkPlayer] SYNCVAR CHANGE: IsReady {prev} -> {next} (asServer: {asServer}, IsOwner: {IsOwner}, ObjectId: {ObjectId})");
        }
        
        private void OnHealthChanged(int prev, int next, bool asServer)
        {
            Debug.Log($"[NetworkPlayer] SYNCVAR CHANGE: CurrentHealth {prev} -> {next} (asServer: {asServer}, IsOwner: {IsOwner}, ObjectId: {ObjectId})");
            OnStateChanged?.Invoke();
        }
        
        private void OnMaxHealthChanged(int prev, int next, bool asServer)
        {
            Debug.Log($"[NetworkPlayer] SYNCVAR CHANGE: MaxHealth {prev} -> {next} (asServer: {asServer}, IsOwner: {IsOwner}, ObjectId: {ObjectId})");
            OnStateChanged?.Invoke();
        }
        
        private void OnManaChanged(int prev, int next, bool asServer)
        {
            Debug.Log($"[NetworkPlayer] SYNCVAR CHANGE: CurrentMana {prev} -> {next} (asServer: {asServer}, IsOwner: {IsOwner}, ObjectId: {ObjectId})");
            OnStateChanged?.Invoke();
        }
        
        private void OnMaxManaChanged(int prev, int next, bool asServer)
        {
            Debug.Log($"[NetworkPlayer] SYNCVAR CHANGE: MaxMana {prev} -> {next} (asServer: {asServer}, IsOwner: {IsOwner}, ObjectId: {ObjectId})");
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
