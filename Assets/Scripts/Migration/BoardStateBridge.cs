using UnityEngine;
using FishNet.Object;
using KOA.Model;
using KOA.Network;
using KOA.View;

namespace KOA.Migration
{
    /// <summary>
    /// Bridge component that connects the old NetworkPlayer/PlayerController system
    /// to the new NetworkBoardState/BoardView system.
    /// 
    /// This allows gradual migration - both systems can run in parallel during transition.
    /// Once migration is complete, this component and the old systems can be removed.
    /// </summary>
    public class BoardStateBridge : NetworkBehaviour
    {
        [Header("New System References")]
        [SerializeField] private BoardView _boardView;
        
        [Header("Old System References")]
        [SerializeField] private PlayerController _localPlayerController;
        [SerializeField] private PlayerController _opponentPlayerController;
        
        [Header("Settings")]
        [SerializeField] private bool _useNewSystem = true;
        [SerializeField] private bool _syncToOldSystem = true;
        
        private NetworkPlayer _localNetworkPlayer;
        private NetworkPlayer _opponentNetworkPlayer;
        
        private void Start()
        {
            // Find references
            FindOldSystemReferences();
            
            if (_boardView == null)
            {
                _boardView = FindObjectOfType<BoardView>();
            }
            
            // Subscribe to new system events
            if (NetworkBoardState.Instance != null)
            {
                NetworkBoardState.Instance.OnStateChanged += OnNewStateChanged;
                NetworkBoardState.Instance.OnCardPlayed += OnNewCardPlayed;
                NetworkBoardState.Instance.OnCardDied += OnNewCardDied;
                NetworkBoardState.Instance.OnTurnChanged += OnNewTurnChanged;
            }
        }
        
        private void OnDestroy()
        {
            if (NetworkBoardState.Instance != null)
            {
                NetworkBoardState.Instance.OnStateChanged -= OnNewStateChanged;
                NetworkBoardState.Instance.OnCardPlayed -= OnNewCardPlayed;
                NetworkBoardState.Instance.OnCardDied -= OnNewCardDied;
                NetworkBoardState.Instance.OnTurnChanged -= OnNewTurnChanged;
            }
        }
        
        private void FindOldSystemReferences()
        {
            // Find NetworkPlayers
            var players = FindObjectsOfType<NetworkPlayer>();
            foreach (var player in players)
            {
                if (player.IsOwner)
                {
                    _localNetworkPlayer = player;
                }
                else
                {
                    _opponentNetworkPlayer = player;
                }
            }
            
            // Find PlayerControllers
            var controllers = FindObjectsOfType<PlayerController>();
            foreach (var controller in controllers)
            {
                if (_localNetworkPlayer != null && controller == _localNetworkPlayer.LinkedPlayerController)
                {
                    _localPlayerController = controller;
                }
                else if (_localPlayerController == null)
                {
                    // Fallback: first controller without a linked network player
                    _localPlayerController = controller;
                }
            }
        }
        
        #region New System Events
        
        private void OnNewStateChanged(BoardState state)
        {
            if (!_syncToOldSystem) return;
            
            // Sync health/mana to old NetworkPlayer
            SyncToOldPlayerStats(state);
        }
        
        private void OnNewCardPlayed(int playerId, int slotIndex, CardState card)
        {
            if (!_syncToOldSystem) return;
            
            Debug.Log($"[BoardStateBridge] Card played in new system: Player {playerId}, Slot {slotIndex}");
            
            // If we wanted to sync card placement to old system, we'd do it here
            // For now, the new system handles this independently
        }
        
        private void OnNewCardDied(int playerId, int slotIndex, CardState card)
        {
            if (!_syncToOldSystem) return;
            
            Debug.Log($"[BoardStateBridge] Card died in new system: Player {playerId}, Slot {slotIndex}");
        }
        
        private void OnNewTurnChanged(int previousPlayerId, int newActivePlayerId)
        {
            if (!_syncToOldSystem) return;
            
            Debug.Log($"[BoardStateBridge] Turn changed from player {previousPlayerId} to {newActivePlayerId}");
            
            // Could sync to EncounterController's turn system
        }
        
        #endregion
        
        #region Sync to Old System
        
        private void SyncToOldPlayerStats(BoardState state)
        {
            if (state == null) return;
            
            // Determine local player ID
            int localPlayerId = _boardView?.LocalPlayerId ?? 0;
            
            // Sync local player stats
            var localState = state.GetPlayer(localPlayerId);
            if (localState != null && _localNetworkPlayer != null)
            {
                // Note: Only server should set these values
                if (IsServerInitialized)
                {
                    _localNetworkPlayer.CurrentHealth.Value = localState.Health;
                    _localNetworkPlayer.CurrentMana.Value = localState.Mana;
                    _localNetworkPlayer.MaxMana.Value = localState.MaxMana;
                }
            }
            
            // Sync opponent stats
            var opponentState = state.GetOpponent(localPlayerId);
            if (opponentState != null && _opponentNetworkPlayer != null)
            {
                if (IsServerInitialized)
                {
                    _opponentNetworkPlayer.CurrentHealth.Value = opponentState.Health;
                    _opponentNetworkPlayer.CurrentMana.Value = opponentState.Mana;
                    _opponentNetworkPlayer.MaxMana.Value = opponentState.MaxMana;
                }
            }
        }
        
        #endregion
        
        #region Import from Old System
        
        /// <summary>
        /// Import the current board state from the old system.
        /// Call this at game start to initialize the new system with existing state.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void CmdImportFromOldSystem()
        {
            if (!IsServerInitialized) return;
            
            Debug.Log("[BoardStateBridge] Importing board state from old system");
            
            // This would read from old PlayerControllers and populate NetworkBoardState
            // Implementation depends on the exact structure of the old system
            
            // Example:
            // var newState = NetworkBoardState.Instance.GetState();
            // newState.GetPlayer(0).Health = _localNetworkPlayer?.CurrentHealth.Value ?? 20;
            // etc.
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Get whether to use the new system for game logic.
        /// </summary>
        public bool UseNewSystem => _useNewSystem;
        
        /// <summary>
        /// Toggle between old and new systems for testing.
        /// </summary>
        public void SetUseNewSystem(bool value)
        {
            _useNewSystem = value;
            Debug.Log($"[BoardStateBridge] UseNewSystem set to {value}");
        }
        
        #endregion
    }
}
