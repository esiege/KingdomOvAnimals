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
                NetworkBoardState.Instance.OnCardDamaged += OnNewCardDamaged;
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
                NetworkBoardState.Instance.OnCardDamaged -= OnNewCardDamaged;
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
            
            // Find PlayerControllers by name (they're always named "Player" and "Opponent" in the scene)
            var playerObj = GameObject.Find("Player");
            var opponentObj = GameObject.Find("Opponent");
            
            if (playerObj != null)
            {
                _localPlayerController = playerObj.GetComponent<PlayerController>();
            }
            if (opponentObj != null)
            {
                _opponentPlayerController = opponentObj.GetComponent<PlayerController>();
            }
            
            Debug.Log($"[BoardStateBridge] Found references: LocalNetworkPlayer={((_localNetworkPlayer != null) ? _localNetworkPlayer.PlayerName.Value : "null")}, " +
                      $"LocalPlayerController={_localPlayerController?.gameObject.name ?? "null"}, " +
                      $"OpponentPlayerController={_opponentPlayerController?.gameObject.name ?? "null"}");
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
        
        private void OnNewCardDamaged(int playerId, int slotIndex, int damage, int newHealth)
        {
            if (!_syncToOldSystem) return;
            
            Debug.Log($"[BoardStateBridge] Card damaged in new system: Player {playerId}, Slot {slotIndex}, Damage={damage}, NewHealth={newHealth}");
            
            // Find and update the card's health in the visual system
            UpdateCardHealthVisual(playerId, slotIndex, damage, newHealth);
        }
        
        /// <summary>
        /// Updates a card's health display in the old CardController system.
        /// </summary>
        private void UpdateCardHealthVisual(int playerId, int slotIndex, int damage, int newHealth)
        {
            // Find the card using the same logic as removal
            int localPlayerId = _localNetworkPlayer?.PlayerId.Value ?? 0;
            bool isLocalPlayerCard = (playerId == localPlayerId);
            
            // Mirror the slot index for opponent cards
            int visualSlotIndex = slotIndex;
            if (!isLocalPlayerCard)
            {
                if (slotIndex == 0) visualSlotIndex = 2;
                else if (slotIndex == 2) visualSlotIndex = 0;
            }
            
            string slotName = isLocalPlayerCard 
                ? $"PlayerSlot-{visualSlotIndex + 1}" 
                : $"OpponentSlot-{visualSlotIndex + 1}";
            
            var slotObj = GameObject.Find(slotName);
            if (slotObj == null)
            {
                Debug.LogWarning($"[BoardStateBridge] Could not find slot '{slotName}' for damage update");
                return;
            }
            
            var cardController = slotObj.GetComponentInChildren<CardController>();
            if (cardController == null)
            {
                Debug.LogWarning($"[BoardStateBridge] No CardController found in slot '{slotName}' for damage update");
                return;
            }
            
            // Update health directly without triggering TakeDamage (which would cause recursive removal)
            int oldHealth = cardController.health;
            cardController.health = newHealth;
            cardController.UpdateCardUI();
            
            Debug.Log($"[BoardStateBridge] Updated '{cardController.cardName}' health: {oldHealth} -> {newHealth}");
        }
        
        private void OnNewCardDied(int playerId, int slotIndex, CardState card)
        {
            if (!_syncToOldSystem) return;
            
            Debug.Log($"[BoardStateBridge] Card died in new system: Player {playerId}, Slot {slotIndex}, Card={card?.CardDataId}");
            
            // Find and remove the card from the visual board
            RemoveCardFromVisualBoard(playerId, slotIndex);
        }
        
        /// <summary>
        /// Removes a card from the visual board using the old CardController system.
        /// Handles perspective conversion from player ID to local slot names.
        /// </summary>
        private void RemoveCardFromVisualBoard(int playerId, int slotIndex)
        {
            // Determine local player ID
            int localPlayerId = _localNetworkPlayer?.PlayerId.Value ?? 0;
            
            // Determine if this is on the local player's side or opponent's side
            bool isLocalPlayerCard = (playerId == localPlayerId);
            
            // Mirror the slot index for opponent cards (visual perspective)
            int visualSlotIndex = slotIndex;
            if (!isLocalPlayerCard)
            {
                // Mirror: 0 <-> 2, 1 stays
                if (slotIndex == 0) visualSlotIndex = 2;
                else if (slotIndex == 2) visualSlotIndex = 0;
            }
            
            // Build the slot name
            string slotName = isLocalPlayerCard 
                ? $"PlayerSlot-{visualSlotIndex + 1}" 
                : $"OpponentSlot-{visualSlotIndex + 1}";
            
            Debug.Log($"[BoardStateBridge] Looking for card in slot: {slotName} (playerId={playerId}, slotIndex={slotIndex}, localPlayerId={localPlayerId})");
            
            // Find the slot
            var slotObj = GameObject.Find(slotName);
            if (slotObj == null)
            {
                Debug.LogWarning($"[BoardStateBridge] Could not find slot '{slotName}'");
                return;
            }
            
            // Find the card in the slot
            var cardController = slotObj.GetComponentInChildren<CardController>();
            if (cardController == null)
            {
                Debug.LogWarning($"[BoardStateBridge] No CardController found in slot '{slotName}'");
                return;
            }
            
            Debug.Log($"[BoardStateBridge] Removing card '{cardController.cardName}' from slot '{slotName}'");
            
            // Get the player controller and remove the card
            var playerController = isLocalPlayerCard ? _localPlayerController : _opponentPlayerController;
            if (playerController != null)
            {
                playerController.RemoveCardFromBoard(cardController);
            }
            else
            {
                // Fallback - just destroy the card
                Debug.LogWarning($"[BoardStateBridge] No PlayerController found, destroying card directly");
                Object.Destroy(cardController.gameObject);
            }
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
