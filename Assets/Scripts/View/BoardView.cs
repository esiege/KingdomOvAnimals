using UnityEngine;
using System.Collections.Generic;
using KOA.Model;
using KOA.Data;
using KOA.Network;

namespace KOA.View
{
    /// <summary>
    /// Manages the visual representation of the game board.
    /// Handles perspective - renders based on LocalPlayerId.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int _localPlayerId = 0;
        
        [Header("Slot Transforms")]
        [Tooltip("Board slots for player 0 (bottom of screen). Index 0=front, 1=middle, 2=back")]
        [SerializeField] private Transform[] _player0Slots = new Transform[3];
        
        [Tooltip("Board slots for player 1 (top of screen). Index 0=front, 1=middle, 2=back")]
        [SerializeField] private Transform[] _player1Slots = new Transform[3];
        
        [Header("Hand Areas")]
        [SerializeField] private Transform _myHandArea;
        [SerializeField] private Transform _opponentHandArea;
        
        [Header("Player Avatars")]
        [Tooltip("Collider for the local player's avatar (for opponent to target)")]
        [SerializeField] private Collider2D _myPlayerAvatarZone;
        
        [Tooltip("Collider for the opponent's avatar (for local player to target)")]
        [SerializeField] private Collider2D _opponentAvatarZone;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject _cardViewPrefab;
        
        [Header("References")]
        [SerializeField] private CardLibrary _cardLibrary;
        
        /// <summary>
        /// Reference to CardLibrary for external access.
        /// </summary>
        public CardLibrary CardLibrary => _cardLibrary;
        
        /// <summary>
        /// Collider for the opponent's avatar zone (for player targeting).
        /// </summary>
        public Collider2D OpponentAvatarZone => _opponentAvatarZone;
        
        /// <summary>
        /// All card views indexed by instance ID.
        /// </summary>
        private Dictionary<int, CardView> _cardViews = new Dictionary<int, CardView>();
        
        /// <summary>
        /// Local player ID (0 or 1). Determines visual perspective.
        /// </summary>
        public int LocalPlayerId
        {
            get => _localPlayerId;
            set
            {
                _localPlayerId = value;
                Debug.Log($"[BoardView] LocalPlayerId set to {value}");
            }
        }
        
        /// <summary>
        /// Returns true if the given player ID is the local player.
        /// </summary>
        public bool IsLocalPlayer(int playerId) => playerId == LocalPlayerId;
        
        /// <summary>
        /// Get the slot transform for a given player and slot index.
        /// Handles visual mirroring - front row for a player appears as back row for opponent.
        /// </summary>
        public Transform GetSlotTransform(int playerId, int slotIndex)
        {
            // Validate
            if (slotIndex < 0 || slotIndex >= 3) return null;
            
            // Determine which side of screen (my side or their side)
            bool isMyCard = IsLocalPlayer(playerId);
            
            // For opponent's cards, we visually mirror front/back
            // Their front (slot 0) appears at our visual back
            int visualSlotIndex = slotIndex;
            if (!isMyCard)
            {
                // Mirror: 0 <-> 2, 1 stays
                if (slotIndex == 0) visualSlotIndex = 2;
                else if (slotIndex == 2) visualSlotIndex = 0;
            }
            
            // Get the appropriate slot array
            Transform[] slots = isMyCard ? GetMySideSlots() : GetTheirSideSlots();
            
            if (slots == null || visualSlotIndex >= slots.Length) return null;
            return slots[visualSlotIndex];
        }
        
        /// <summary>
        /// Get slots on my side of the screen.
        /// </summary>
        private Transform[] GetMySideSlots()
        {
            // Local player 0 uses player0 slots on bottom
            // Local player 1 uses player1 slots on bottom (their view)
            return LocalPlayerId == 0 ? _player0Slots : _player1Slots;
        }
        
        /// <summary>
        /// Get slots on opponent's side of the screen.
        /// </summary>
        private Transform[] GetTheirSideSlots()
        {
            return LocalPlayerId == 0 ? _player1Slots : _player0Slots;
        }
        
        /// <summary>
        /// Get hand area for a player.
        /// </summary>
        public Transform GetHandArea(int playerId)
        {
            return IsLocalPlayer(playerId) ? _myHandArea : _opponentHandArea;
        }
        
        #region Card View Management
        
        /// <summary>
        /// Create a new card view for a card state.
        /// </summary>
        public CardView CreateCardView(CardState state)
        {
            if (state == null || !state.IsValid)
            {
                Debug.LogWarning("[BoardView] Cannot create view for invalid state");
                return null;
            }
            
            if (_cardViews.ContainsKey(state.InstanceId))
            {
                Debug.LogWarning($"[BoardView] CardView already exists for instance {state.InstanceId}");
                return _cardViews[state.InstanceId];
            }
            
            Debug.Log($"[BoardView] CreateCardView: Instantiating cardDataId={state.CardDataId}, instanceId={state.InstanceId}");
            var viewObj = Instantiate(_cardViewPrefab);
            var view = viewObj.GetComponent<CardView>();
            if (view == null)
            {
                Debug.LogError("[BoardView] CardView prefab missing CardView component");
                Destroy(viewObj);
                return null;
            }
            
            view.Initialize(state, _cardLibrary);
            _cardViews[state.InstanceId] = view;
            Debug.Log($"[BoardView] Created card view {state.InstanceId}, total views: {_cardViews.Count}");
            
            return view;
        }
        
        /// <summary>
        /// Get all card views.
        /// </summary>
        public IReadOnlyDictionary<int, CardView> GetAllCardViews() => _cardViews;
        
        /// <summary>
        /// Get existing card view by instance ID.
        /// </summary>
        public CardView GetCardView(int instanceId)
        {
            _cardViews.TryGetValue(instanceId, out var view);
            return view;
        }
        
        /// <summary>
        /// Remove and destroy a card view.
        /// </summary>
        public void DestroyCardView(int instanceId)
        {
            if (_cardViews.TryGetValue(instanceId, out var view))
            {
                _cardViews.Remove(instanceId);
                if (view != null)
                {
                    view.PlayDeathAnimation(() => Destroy(view.gameObject));
                }
            }
        }
        
        /// <summary>
        /// Place a card view in a board slot.
        /// </summary>
        public void PlaceCardInSlot(CardView view, int playerId, int slotIndex)
        {
            if (view == null) return;
            
            var slot = GetSlotTransform(playerId, slotIndex);
            if (slot == null)
            {
                Debug.LogError($"[BoardView] Could not find slot for ({playerId}, {slotIndex})");
                return;
            }
            
            view.transform.SetParent(slot);
            view.transform.localPosition = Vector3.zero;
            view.transform.localRotation = Quaternion.identity;
            view.transform.localScale = Vector3.one;
            view.SetBoardPosition(playerId, slotIndex);
            
            Debug.Log($"[BoardView] Placed card {view.InstanceId} in slot ({playerId}, {slotIndex}) -> visual slot {slot.name}");
        }
        
        /// <summary>
        /// Place a card view in hand.
        /// </summary>
        public void PlaceCardInHand(CardView view, int playerId, int handIndex)
        {
            if (view == null) return;
            
            var handArea = GetHandArea(playerId);
            if (handArea == null)
            {
                Debug.LogError($"[BoardView] Could not find hand area for player {playerId}");
                return;
            }
            
            view.transform.SetParent(handArea);
            view.SetHandPosition(handIndex);
            
            // TODO: Position card properly in hand fan
            UpdateHandLayout(playerId);
        }
        
        /// <summary>
        /// Update hand card positions.
        /// </summary>
        public void UpdateHandLayout(int playerId)
        {
            var handArea = GetHandArea(playerId);
            if (handArea == null)
            {
                Debug.LogError($"[BoardView] UpdateHandLayout: handArea is null for player {playerId}");
                return;
            }
            
            Debug.Log($"[BoardView] UpdateHandLayout: player {playerId}, handArea at {handArea.position}");
            
            var handCards = new List<CardView>();
            foreach (var kvp in _cardViews)
            {
                Debug.Log($"[BoardView] Checking card {kvp.Key}: IsInHand={kvp.Value.IsInHand}, OwnerId={kvp.Value.OwnerId}, BoardPos={kvp.Value.BoardPosition}");
                if (kvp.Value.IsInHand && kvp.Value.OwnerId == playerId)
                {
                    handCards.Add(kvp.Value);
                }
            }
            
            Debug.Log($"[BoardView] UpdateHandLayout: Found {handCards.Count} cards for player {playerId}'s hand");
            
            // Sort by hand index
            handCards.Sort((a, b) => a.HandIndex.CompareTo(b.HandIndex));
            
            // Position cards - use smaller spacing for 2D
            float spacing = 1.5f; // World units between cards
            float startX = -(handCards.Count - 1) * spacing / 2f;
            
            for (int i = 0; i < handCards.Count; i++)
            {
                var card = handCards[i];
                card.transform.localPosition = new Vector3(startX + i * spacing, 0, 0);
                card.transform.localRotation = Quaternion.identity;
                card.transform.localScale = Vector3.one;
                Debug.Log($"[BoardView] Positioned card {card.InstanceId} at local ({startX + i * spacing}, 0, 0), world {card.transform.position}");
            }
        }
        
        #endregion
        
        #region Full Board Render
        
        /// <summary>
        /// Render the full board state. Creates/updates/destroys card views as needed.
        /// </summary>
        public void RenderBoard(BoardState state)
        {
            if (state == null) return;
            
            Debug.Log($"[BoardView] RenderBoard: Turn {state.TurnNumber}, Current player {state.CurrentTurnPlayerId}");
            Debug.Log($"[BoardView] Player 0 hand count: {state.GetPlayer(0).Hand.Count}");
            Debug.Log($"[BoardView] Player 1 hand count: {state.GetPlayer(1).Hand.Count}");
            
            // Track which cards we've seen
            var seenInstanceIds = new HashSet<int>();
            
            // Render each player's board
            for (int playerId = 0; playerId < 2; playerId++)
            {
                var player = state.GetPlayer(playerId);
                Debug.Log($"[BoardView] Rendering player {playerId}: {player.Hand.Count} cards in hand, checking board slots");
                
                // Render board cards
                for (int slot = 0; slot < 3; slot++)
                {
                    var cardState = player.GetCardInSlot(slot);
                    if (cardState != null && cardState.IsValid)
                    {
                        seenInstanceIds.Add(cardState.InstanceId);
                        
                        var view = GetCardView(cardState.InstanceId);
                        if (view == null)
                        {
                            view = CreateCardView(cardState);
                        }
                        
                        if (view != null)
                        {
                            view.UpdateFromState(cardState);
                            PlaceCardInSlot(view, playerId, slot);
                        }
                    }
                }
                
                // Hand cards are rendered by HandView - don't duplicate here
                // HandView subscribes to OnStateChanged and handles hand layout
            }
            
            // Destroy views for cards that no longer exist
            var toRemove = new List<int>();
            foreach (var kvp in _cardViews)
            {
                if (!seenInstanceIds.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var id in toRemove)
            {
                DestroyCardView(id);
            }
        }
        
        #endregion
        
        #region Highlighting
        
        /// <summary>
        /// Highlight valid target slots.
        /// </summary>
        public void HighlightSlots(List<(int playerId, int slotIndex)> slots, bool highlight)
        {
            foreach (var (playerId, slotIndex) in slots)
            {
                var card = GetCardInSlot(playerId, slotIndex);
                if (card != null)
                {
                    card.SetHighlight(highlight);
                }
            }
        }
        
        /// <summary>
        /// Clear all highlights.
        /// </summary>
        public void ClearAllHighlights()
        {
            foreach (var view in _cardViews.Values)
            {
                view.SetHighlight(false);
            }
        }
        
        /// <summary>
        /// Get card view in a specific slot.
        /// </summary>
        public CardView GetCardInSlot(int playerId, int slotIndex)
        {
            foreach (var view in _cardViews.Values)
            {
                if (view.BoardPosition.playerId == playerId && view.BoardPosition.slotIndex == slotIndex)
                {
                    return view;
                }
            }
            return null;
        }
        
        #endregion
        
        #region Setup
        
        private void Start()
        {
            // Find local player ID from network
            FindAndSetLocalPlayerId();
            
            // Subscribe to NetworkBoardState events
            if (NetworkBoardState.Instance != null)
            {
                NetworkBoardState.Instance.OnStateChanged += OnStateChanged;
                NetworkBoardState.Instance.OnCardPlayed += OnCardPlayed;
                NetworkBoardState.Instance.OnCardDamaged += OnCardDamaged;
                NetworkBoardState.Instance.OnCardDied += OnCardDied;
                
                // Initial render
                RenderBoard(NetworkBoardState.Instance.GetState());
            }
        }
        
        /// <summary>
        /// Find the local NetworkPlayer and set LocalPlayerId.
        /// </summary>
        private void FindAndSetLocalPlayerId()
        {
            // Find all NetworkPlayers and look for the one we own
            var networkPlayers = FindObjectsOfType<KOA.Network.NetworkPlayer>();
            foreach (var np in networkPlayers)
            {
                if (np.IsOwner)
                {
                    LocalPlayerId = np.PlayerId.Value;
                    Debug.Log($"[BoardView] Found local NetworkPlayer with PlayerId {LocalPlayerId}");
                    return;
                }
            }
            
            // Fallback: check NetworkGameManager
            if (NetworkGameManager.Instance != null)
            {
                var localNp = NetworkGameManager.Instance.GetLocalPlayer();
                if (localNp != null)
                {
                    LocalPlayerId = localNp.PlayerId.Value;
                    Debug.Log($"[BoardView] Got LocalPlayerId {LocalPlayerId} from NetworkGameManager");
                    return;
                }
            }
            
            Debug.LogWarning("[BoardView] Could not find local NetworkPlayer - defaulting to player 0");
        }
        
        private void OnDestroy()
        {
            if (NetworkBoardState.Instance != null)
            {
                NetworkBoardState.Instance.OnStateChanged -= OnStateChanged;
                NetworkBoardState.Instance.OnCardPlayed -= OnCardPlayed;
                NetworkBoardState.Instance.OnCardDamaged -= OnCardDamaged;
                NetworkBoardState.Instance.OnCardDied -= OnCardDied;
            }
        }
        
        private void OnStateChanged(BoardState state)
        {
            RenderBoard(state);
        }
        
        private void OnCardPlayed(int playerId, int slotIndex, CardState card)
        {
            // Card placement handled by state change
        }
        
        private void OnCardDamaged(int playerId, int slotIndex, int damage, int newHealth)
        {
            var view = GetCardInSlot(playerId, slotIndex);
            if (view != null)
            {
                view.PlayDamageAnimation(damage);
            }
        }
        
        private void OnCardDied(int playerId, int slotIndex, CardState card)
        {
            // Card removal handled by state change
        }
        
        #endregion
    }
}
