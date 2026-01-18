using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using KOA.Model;
using KOA.Data;
using System.Collections.Generic;
using UnityEngine;

namespace KOA.Network
{
    /// <summary>
    /// Network-synchronized board state. This is the single source of truth for all game state.
    /// Server authoritative - only server modifies state, clients receive updates.
    /// </summary>
    public class NetworkBoardState : NetworkBehaviour
    {
        #region Singleton
        
        public static NetworkBoardState Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        #endregion
        
        #region Synced State
        
        /// <summary>
        /// The complete board state, synced to all clients.
        /// </summary>
        public readonly SyncVar<BoardState> State = new SyncVar<BoardState>(new BoardState());
        
        /// <summary>
        /// Card library for looking up CardData by ID.
        /// </summary>
        [SerializeField] private CardLibrary _cardLibrary;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Fired when any part of the board state changes.
        /// </summary>
        public System.Action<BoardState> OnStateChanged;
        
        /// <summary>
        /// Fired when a card is played to the board.
        /// Args: playerId, slotIndex, cardState
        /// </summary>
        public System.Action<int, int, CardState> OnCardPlayed;
        
        /// <summary>
        /// Fired when a card takes damage.
        /// Args: playerId, slotIndex, damage, newHealth
        /// </summary>
        public System.Action<int, int, int, int> OnCardDamaged;
        
        /// <summary>
        /// Fired when a card dies.
        /// Args: playerId, slotIndex, cardState
        /// </summary>
        public System.Action<int, int, CardState> OnCardDied;
        
        /// <summary>
        /// Fired when a card is drawn.
        /// Args: playerId, cardState
        /// </summary>
        public System.Action<int, CardState> OnCardDrawn;
        
        /// <summary>
        /// Fired when the turn changes.
        /// Args: newCurrentPlayerId, turnNumber
        /// </summary>
        public System.Action<int, int> OnTurnChanged;
        
        /// <summary>
        /// Fired when a player takes damage.
        /// Args: playerId, damage, newHealth
        /// </summary>
        public System.Action<int, int, int> OnPlayerDamaged;
        
        /// <summary>
        /// Fired when the game ends.
        /// Args: winnerPlayerId
        /// </summary>
        public System.Action<int> OnGameEnded;
        
        #endregion
        
        #region Initialization
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("[NetworkBoardState] Server started");
        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            State.OnChange += OnBoardStateChanged;
            Debug.Log("[NetworkBoardState] Client started, subscribed to state changes");
        }
        
        public override void OnStopClient()
        {
            base.OnStopClient();
            State.OnChange -= OnBoardStateChanged;
        }
        
        private void OnBoardStateChanged(BoardState prev, BoardState next, bool asServer)
        {
            Debug.Log($"[NetworkBoardState] State changed (asServer={asServer}): {next}");
            OnStateChanged?.Invoke(next);
        }
        
        /// <summary>
        /// Initialize the game with two players and their decks.
        /// Server only.
        /// </summary>
        [Server]
        public void InitializeGame(List<string> player0Deck, List<string> player1Deck)
        {
            Debug.Log("[NetworkBoardState] Initializing game");
            
            var newState = new BoardState();
            newState.Players[0].Deck = new List<string>(player0Deck);
            newState.Players[1].Deck = new List<string>(player1Deck);
            
            // Shuffle decks
            ShuffleDeck(newState.Players[0].Deck);
            ShuffleDeck(newState.Players[1].Deck);
            
            // Draw initial hands (4 cards each)
            for (int i = 0; i < 4; i++)
            {
                DrawCardInternal(newState, 0);
                DrawCardInternal(newState, 1);
            }
            
            newState.StartGame();
            State.Value = newState;
            
            // Notify clients
            RpcGameStarted();
        }
        
        private void ShuffleDeck(List<string> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }
        
        #endregion
        
        #region Card Play
        
        /// <summary>
        /// Request to play a card from hand to a board slot.
        /// Called by client, validated and executed on server.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void CmdPlayCard(int playerId, int handIndex, int slotIndex, NetworkConnection conn = null)
        {
            Debug.Log($"[NetworkBoardState] CmdPlayCard: player={playerId}, hand={handIndex}, slot={slotIndex}");
            
            var state = State.Value;
            
            // Validate turn
            if (!state.IsPlayerTurn(playerId))
            {
                Debug.LogWarning($"[NetworkBoardState] Not player {playerId}'s turn");
                return;
            }
            
            var player = state.GetPlayer(playerId);
            
            // Validate hand index
            var card = player.GetHandCard(handIndex);
            if (card == null)
            {
                Debug.LogWarning($"[NetworkBoardState] Invalid hand index {handIndex}");
                return;
            }
            
            // Validate slot
            if (!player.IsSlotEmpty(slotIndex))
            {
                Debug.LogWarning($"[NetworkBoardState] Slot {slotIndex} is not empty");
                return;
            }
            
            // Validate mana
            var cardData = _cardLibrary.GetCardById(card.CardDataId);
            if (cardData == null || !player.CanAfford(cardData.manaCost))
            {
                Debug.LogWarning($"[NetworkBoardState] Cannot afford card");
                return;
            }
            
            // Execute play
            player.SpendMana(cardData.manaCost);
            player.RemoveHandCard(handIndex);
            card.HasSummoningSickness = true;
            player.PlaceCard(slotIndex, card);
            
            // Update synced state
            State.Value = state;
            
            Debug.Log($"[NetworkBoardState] Card played: {card.CardDataId} to slot {slotIndex}");
            
            // Notify clients
            RpcCardPlayed(playerId, slotIndex, card);
        }
        
        [ObserversRpc]
        private void RpcCardPlayed(int playerId, int slotIndex, CardState card)
        {
            Debug.Log($"[Client] Card played: player={playerId}, slot={slotIndex}, card={card.CardDataId}");
            OnCardPlayed?.Invoke(playerId, slotIndex, card);
        }
        
        #endregion
        
        #region Abilities
        
        /// <summary>
        /// Request to use a flip ability (from hand) on a target.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void CmdUseFlipAbility(int attackerPlayerId, int handIndex, int targetPlayerId, int targetSlotIndex, NetworkConnection conn = null)
        {
            Debug.Log($"[NetworkBoardState] CmdUseFlipAbility: attacker={attackerPlayerId}, hand={handIndex}, target=({targetPlayerId}, {targetSlotIndex})");
            
            var state = State.Value;
            
            // Validate turn
            if (!state.IsPlayerTurn(attackerPlayerId))
            {
                Debug.LogWarning("[NetworkBoardState] Not player's turn");
                return;
            }
            
            var attacker = state.GetPlayer(attackerPlayerId);
            var attackerCard = attacker.GetHandCard(handIndex);
            if (attackerCard == null)
            {
                Debug.LogWarning("[NetworkBoardState] Invalid hand index");
                return;
            }
            
            // Get ability damage
            var cardData = _cardLibrary.GetCardById(attackerCard.CardDataId);
            if (cardData == null || cardData.offensiveAbility == null)
            {
                Debug.LogWarning("[NetworkBoardState] Card has no offensive ability");
                return;
            }
            
            int damage = cardData.offensiveAbility.damage;
            
            // Validate mana
            if (!attacker.CanAfford(cardData.manaCost))
            {
                Debug.LogWarning("[NetworkBoardState] Cannot afford flip ability");
                return;
            }
            
            // Apply damage to target
            var targetPlayer = state.GetPlayer(targetPlayerId);
            var targetCard = targetPlayer.GetCardInSlot(targetSlotIndex);
            if (targetCard == null)
            {
                Debug.LogWarning($"[NetworkBoardState] No card in target slot ({targetPlayerId}, {targetSlotIndex})");
                return;
            }
            
            // Execute
            attacker.SpendMana(cardData.manaCost);
            attackerCard.IsFlipped = true;
            
            bool died = targetCard.TakeDamage(damage);
            
            Debug.Log($"[NetworkBoardState] {attackerCard.CardDataId} deals {damage} to {targetCard.CardDataId}, died={died}");
            
            // Update state
            State.Value = state;
            
            // Notify damage
            RpcCardDamaged(targetPlayerId, targetSlotIndex, damage, targetCard.CurrentHealth);
            
            // Handle death
            if (died)
            {
                targetPlayer.RemoveCard(targetSlotIndex);
                State.Value = state;
                RpcCardDied(targetPlayerId, targetSlotIndex, targetCard);
            }
            
            state.CheckGameOver();
            if (!state.IsGameActive)
            {
                State.Value = state;
                RpcGameEnded(state.WinnerPlayerId);
            }
        }
        
        /// <summary>
        /// Request to use a board ability (from board card) on a target.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void CmdUseBoardAbility(int attackerPlayerId, int attackerSlotIndex, int targetPlayerId, int targetSlotIndex, NetworkConnection conn = null)
        {
            Debug.Log($"[NetworkBoardState] CmdUseBoardAbility: attacker=({attackerPlayerId}, {attackerSlotIndex}), target=({targetPlayerId}, {targetSlotIndex})");
            
            var state = State.Value;
            
            // Validate turn
            if (!state.IsPlayerTurn(attackerPlayerId))
            {
                Debug.LogWarning("[NetworkBoardState] Not player's turn");
                return;
            }
            
            var attacker = state.GetPlayer(attackerPlayerId);
            var attackerCard = attacker.GetCardInSlot(attackerSlotIndex);
            if (attackerCard == null || !attackerCard.CanAct)
            {
                Debug.LogWarning("[NetworkBoardState] Card cannot act");
                return;
            }
            
            // Get ability damage
            var cardData = _cardLibrary.GetCardById(attackerCard.CardDataId);
            if (cardData == null || cardData.offensiveAbility == null)
            {
                Debug.LogWarning("[NetworkBoardState] Card has no offensive ability");
                return;
            }
            
            int damage = cardData.offensiveAbility.damage;
            
            // Apply damage to target
            var targetPlayer = state.GetPlayer(targetPlayerId);
            var targetCard = targetPlayer.GetCardInSlot(targetSlotIndex);
            if (targetCard == null)
            {
                Debug.LogWarning($"[NetworkBoardState] No card in target slot ({targetPlayerId}, {targetSlotIndex})");
                return;
            }
            
            // Execute
            attackerCard.IsTapped = true;
            bool died = targetCard.TakeDamage(damage);
            
            Debug.Log($"[NetworkBoardState] {attackerCard.CardDataId} deals {damage} to {targetCard.CardDataId}, died={died}");
            
            // Update state
            State.Value = state;
            
            // Notify damage
            RpcCardDamaged(targetPlayerId, targetSlotIndex, damage, targetCard.CurrentHealth);
            
            // Handle death
            if (died)
            {
                targetPlayer.RemoveCard(targetSlotIndex);
                State.Value = state;
                RpcCardDied(targetPlayerId, targetSlotIndex, targetCard);
            }
            
            state.CheckGameOver();
            if (!state.IsGameActive)
            {
                State.Value = state;
                RpcGameEnded(state.WinnerPlayerId);
            }
        }
        
        /// <summary>
        /// Request to attack player directly.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void CmdAttackPlayer(int attackerPlayerId, int attackerSlotIndex, int targetPlayerId, NetworkConnection conn = null)
        {
            Debug.Log($"[NetworkBoardState] CmdAttackPlayer: attacker=({attackerPlayerId}, {attackerSlotIndex}), target={targetPlayerId}");
            
            var state = State.Value;
            
            // Validate turn
            if (!state.IsPlayerTurn(attackerPlayerId))
            {
                Debug.LogWarning("[NetworkBoardState] Not player's turn");
                return;
            }
            
            var attacker = state.GetPlayer(attackerPlayerId);
            var attackerCard = attacker.GetCardInSlot(attackerSlotIndex);
            if (attackerCard == null || !attackerCard.CanAct)
            {
                Debug.LogWarning("[NetworkBoardState] Card cannot act");
                return;
            }
            
            // Get attack damage
            var cardData = _cardLibrary.GetCardById(attackerCard.CardDataId);
            if (cardData == null || cardData.offensiveAbility == null)
            {
                Debug.LogWarning("[NetworkBoardState] Card has no offensive ability");
                return;
            }
            
            int damage = cardData.offensiveAbility.damage;
            
            // Execute
            attackerCard.IsTapped = true;
            var targetPlayer = state.GetPlayer(targetPlayerId);
            targetPlayer.TakeDamage(damage);
            
            Debug.Log($"[NetworkBoardState] {attackerCard.CardDataId} deals {damage} to player {targetPlayerId}");
            
            // Update state
            State.Value = state;
            
            // Notify
            RpcPlayerDamaged(targetPlayerId, damage, targetPlayer.Health);
            
            state.CheckGameOver();
            if (!state.IsGameActive)
            {
                State.Value = state;
                RpcGameEnded(state.WinnerPlayerId);
            }
        }
        
        /// <summary>
        /// Request to use flip ability on player directly.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void CmdUseFlipAbilityOnPlayer(int attackerPlayerId, int handIndex, int targetPlayerId, NetworkConnection conn = null)
        {
            Debug.Log($"[NetworkBoardState] CmdUseFlipAbilityOnPlayer: attacker={attackerPlayerId}, hand={handIndex}, target={targetPlayerId}");
            
            var state = State.Value;
            
            // Validate turn
            if (!state.IsPlayerTurn(attackerPlayerId))
            {
                Debug.LogWarning("[NetworkBoardState] Not player's turn");
                return;
            }
            
            var attacker = state.GetPlayer(attackerPlayerId);
            var attackerCard = attacker.GetHandCard(handIndex);
            if (attackerCard == null)
            {
                Debug.LogWarning("[NetworkBoardState] Invalid hand index");
                return;
            }
            
            // Get ability damage
            var cardData = _cardLibrary.GetCardById(attackerCard.CardDataId);
            if (cardData == null || cardData.offensiveAbility == null)
            {
                Debug.LogWarning("[NetworkBoardState] Card has no offensive ability");
                return;
            }
            
            int damage = cardData.offensiveAbility.damage;
            
            // Validate mana
            if (!attacker.CanAfford(cardData.manaCost))
            {
                Debug.LogWarning("[NetworkBoardState] Cannot afford flip ability");
                return;
            }
            
            // Execute
            attacker.SpendMana(cardData.manaCost);
            attackerCard.IsFlipped = true;
            
            var targetPlayer = state.GetPlayer(targetPlayerId);
            targetPlayer.TakeDamage(damage);
            
            Debug.Log($"[NetworkBoardState] {attackerCard.CardDataId} deals {damage} to player {targetPlayerId}");
            
            // Update state
            State.Value = state;
            
            // Notify
            RpcPlayerDamaged(targetPlayerId, damage, targetPlayer.Health);
            
            state.CheckGameOver();
            if (!state.IsGameActive)
            {
                State.Value = state;
                RpcGameEnded(state.WinnerPlayerId);
            }
        }
        
        [ObserversRpc]
        private void RpcCardDamaged(int playerId, int slotIndex, int damage, int newHealth)
        {
            Debug.Log($"[Client] Card damaged: ({playerId}, {slotIndex}) took {damage}, now at {newHealth}");
            OnCardDamaged?.Invoke(playerId, slotIndex, damage, newHealth);
        }
        
        [ObserversRpc]
        private void RpcCardDied(int playerId, int slotIndex, CardState card)
        {
            Debug.Log($"[Client] Card died: ({playerId}, {slotIndex}) - {card.CardDataId}");
            OnCardDied?.Invoke(playerId, slotIndex, card);
        }
        
        [ObserversRpc]
        private void RpcPlayerDamaged(int playerId, int damage, int newHealth)
        {
            Debug.Log($"[Client] Player damaged: {playerId} took {damage}, now at {newHealth}");
            OnPlayerDamaged?.Invoke(playerId, damage, newHealth);
        }
        
        #endregion
        
        #region Turn Management
        
        /// <summary>
        /// Request to end turn.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void CmdEndTurn(int playerId, NetworkConnection conn = null)
        {
            Debug.Log($"[NetworkBoardState] CmdEndTurn: player={playerId}");
            
            var state = State.Value;
            
            if (!state.IsPlayerTurn(playerId))
            {
                Debug.LogWarning("[NetworkBoardState] Not player's turn");
                return;
            }
            
            state.EndTurn();
            
            // Draw card for new player
            DrawCardInternal(state, state.CurrentTurnPlayerId);
            
            State.Value = state;
            
            RpcTurnChanged(state.CurrentTurnPlayerId, state.TurnNumber);
        }
        
        [ObserversRpc]
        private void RpcTurnChanged(int newCurrentPlayerId, int turnNumber)
        {
            Debug.Log($"[Client] Turn changed: player {newCurrentPlayerId}'s turn, turn #{turnNumber}");
            OnTurnChanged?.Invoke(newCurrentPlayerId, turnNumber);
        }
        
        #endregion
        
        #region Card Draw
        
        private CardState DrawCardInternal(BoardState state, int playerId)
        {
            var player = state.GetPlayer(playerId);
            var card = player.DrawCard(_cardLibrary);
            if (card != null)
            {
                Debug.Log($"[NetworkBoardState] Player {playerId} drew {card.CardDataId}");
            }
            return card;
        }
        
        [ObserversRpc]
        private void RpcCardDrawn(int playerId, CardState card)
        {
            Debug.Log($"[Client] Card drawn: player={playerId}, card={card.CardDataId}");
            OnCardDrawn?.Invoke(playerId, card);
        }
        
        [ObserversRpc]
        private void RpcGameStarted()
        {
            Debug.Log("[Client] Game started!");
        }
        
        [ObserversRpc]
        private void RpcGameEnded(int winnerPlayerId)
        {
            Debug.Log($"[Client] Game ended! Winner: Player {winnerPlayerId}");
            OnGameEnded?.Invoke(winnerPlayerId);
        }
        
        #endregion
        
        #region Queries (Read-only access to state)
        
        /// <summary>
        /// Get current board state (read-only).
        /// </summary>
        public BoardState GetState() => State.Value;
        
        /// <summary>
        /// Get a player's state.
        /// </summary>
        public PlayerBoardState GetPlayerState(int playerId) => State.Value?.GetPlayer(playerId);
        
        /// <summary>
        /// Check if it's a player's turn.
        /// </summary>
        public bool IsPlayerTurn(int playerId) => State.Value?.IsPlayerTurn(playerId) ?? false;
        
        /// <summary>
        /// Get current turn player ID.
        /// </summary>
        public int GetCurrentTurnPlayerId() => State.Value?.CurrentTurnPlayerId ?? -1;
        
        #endregion
    }
}
