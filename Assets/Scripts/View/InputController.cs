using UnityEngine;
using System.Collections.Generic;
using KOA.Model;
using KOA.Network;
using KOA.Data;

namespace KOA.View
{
    /// <summary>
    /// Handles player input for the board.
    /// Translates mouse/touch input into game commands.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private HandView _myHandView;
        [SerializeField] private CardLibrary _cardLibrary;
        [SerializeField] private LineRenderer _targetingLine;
        
        [Header("Input Settings")]
        [SerializeField] private LayerMask _cardLayerMask;
        [SerializeField] private LayerMask _slotLayerMask;
        [SerializeField] private LayerMask _playerAvatarLayerMask;
        
        private enum InputState
        {
            Idle,
            DraggingHandCard,
            DraggingBoardCard,
            SelectingTarget,
            SelectingSupportTarget  // New: For defensive/support ability targeting
        }
        
        private InputState _state = InputState.Idle;
        private CardView _selectedCard;
        private Vector3 _dragStartPosition;
        private int _selectedHandIndex = -1;
        private int _selectedBoardSlot = -1;
        private bool _usingSupportAbility = false;  // Track if using defensive ability
        
        /// <summary>
        /// Local player ID.
        /// </summary>
        public int LocalPlayerId => _boardView?.LocalPlayerId ?? 0;
        
        private void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            // Don't process input if not our turn
            if (!IsMyTurn())
            {
                // Only log occasionally to avoid spam
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    Debug.Log($"[InputController] Not my turn. LocalPlayerId={LocalPlayerId}, CurrentTurn={NetworkBoardState.Instance?.State.Value?.CurrentTurnPlayerId}");
                }
                return;
            }
            
            switch (_state)
            {
                case InputState.Idle:
                    HandleIdleInput();
                    break;
                case InputState.DraggingHandCard:
                    HandleDraggingHandCard();
                    break;
                case InputState.DraggingBoardCard:
                    HandleDraggingBoardCard();
                    break;
                case InputState.SelectingTarget:
                    HandleSelectingTarget();
                    break;
                case InputState.SelectingSupportTarget:
                    HandleSelectingSupportTarget();
                    break;
            }
        }
        
        private bool IsMyTurn()
        {
            return NetworkBoardState.Instance?.IsPlayerTurn(LocalPlayerId) ?? false;
        }
        
        #region Idle State
        
        private void HandleIdleInput()
        {
            // Left click: offensive actions (attack, play card)
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"[InputController] Left click detected");
                var card = GetCardUnderMouse();
                if (card != null)
                {
                    Debug.Log($"[InputController] Found card under mouse: {card.CardDataId} (Instance {card.InstanceId})");
                    OnCardClicked(card, useSupport: false);
                }
                else
                {
                    Debug.Log($"[InputController] No card under mouse");
                }
            }
            
            // Right click: support/defensive actions (heal, buff)
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log($"[InputController] Right click detected");
                var card = GetCardUnderMouse();
                if (card != null)
                {
                    Debug.Log($"[InputController] Found card under mouse: {card.CardDataId} (Instance {card.InstanceId})");
                    OnCardClicked(card, useSupport: true);
                }
            }
        }
        
        private void OnCardClicked(CardView card, bool useSupport)
        {
            // Is this our card?
            if (card.OwnerId != LocalPlayerId)
            {
                Debug.Log("[InputController] Can't select opponent's card");
                return;
            }
            
            if (card.IsInHand)
            {
                // Start dragging hand card
                _selectedCard = card;
                _selectedHandIndex = card.HandIndex;
                _dragStartPosition = card.transform.position;
                _state = InputState.DraggingHandCard;
                _usingSupportAbility = false;
                
                // Highlight valid targets
                HighlightValidPlaySlots();
                
                Debug.Log($"[InputController] Started dragging hand card {card.InstanceId}");
            }
            else
            {
                // Check if card can act
                var state = NetworkBoardState.Instance?.GetState();
                var cardState = state?.GetCard(card.BoardPosition.playerId, card.BoardPosition.slotIndex);
                
                if (cardState != null && cardState.CanAct)
                {
                    _selectedCard = card;
                    _selectedBoardSlot = card.BoardPosition.slotIndex;
                    _usingSupportAbility = useSupport;
                    
                    if (useSupport)
                    {
                        // Check if card has a defensive ability
                        var cardData = _cardLibrary?.GetCardById(cardState.CardDataId);
                        if (cardData?.defensiveAbility == null)
                        {
                            Debug.Log("[InputController] Card has no support ability");
                            return;
                        }
                        
                        _state = InputState.SelectingSupportTarget;
                        HighlightValidSupportTargets();
                        Debug.Log($"[InputController] Selecting support target for {card.InstanceId}");
                    }
                    else
                    {
                        _state = InputState.DraggingBoardCard;
                        HighlightValidAbilityTargets();
                        Debug.Log($"[InputController] Started dragging board card {card.InstanceId}");
                    }
                }
                else
                {
                    Debug.Log("[InputController] Card cannot act (tapped, sickness, or frozen)");
                }
            }
        }
        
        #endregion
        
        #region Dragging Hand Card
        
        private void HandleDraggingHandCard()
        {
            // Move card with mouse
            if (_selectedCard != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = _selectedCard.transform.position.z;
                _selectedCard.transform.position = mousePos;
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                // Check if over a valid slot
                int targetSlot = GetSlotUnderMouse();
                
                if (targetSlot >= 0 && CanPlayToSlot(targetSlot))
                {
                    // Play card
                    PlayCardToSlot(_selectedHandIndex, targetSlot);
                }
                else
                {
                    // Check if over valid flip target
                    var targetCard = GetCardUnderMouse();
                    if (targetCard != null && targetCard.OwnerId != LocalPlayerId)
                    {
                        // Flip ability attack on enemy card
                        UseFlipAbilityOnCard(_selectedHandIndex, targetCard);
                    }
                    else if (IsOverOpponentAvatar())
                    {
                        // Flip ability on opponent player directly
                        UseFlipAbilityOnPlayer(_selectedHandIndex);
                    }
                    else
                    {
                        // Return card to hand
                        _selectedCard.transform.position = _dragStartPosition;
                    }
                }
                
                ClearSelection();
            }
        }
        
        private void HighlightValidPlaySlots()
        {
            var state = NetworkBoardState.Instance?.GetState();
            if (state == null) return;
            
            var player = state.GetPlayer(LocalPlayerId);
            var validSlots = new List<(int, int)>();
            
            for (int i = 0; i < 3; i++)
            {
                if (player.IsSlotEmpty(i))
                {
                    validSlots.Add((LocalPlayerId, i));
                }
            }
            
            // Also highlight enemy cards as potential flip targets
            var enemy = state.GetOpponent(LocalPlayerId);
            for (int i = 0; i < 3; i++)
            {
                if (!enemy.IsSlotEmpty(i))
                {
                    validSlots.Add((1 - LocalPlayerId, i));
                }
            }
            
            _boardView?.HighlightSlots(validSlots, true);
        }
        
        private bool CanPlayToSlot(int slotIndex)
        {
            var state = NetworkBoardState.Instance?.GetState();
            if (state == null) return false;
            
            var player = state.GetPlayer(LocalPlayerId);
            if (!player.IsSlotEmpty(slotIndex)) return false;
            
            // Check mana
            var handCard = player.GetHandCard(_selectedHandIndex);
            if (handCard == null) return false;
            
            var cardData = _cardLibrary?.GetCardById(handCard.CardDataId);
            if (cardData == null) return false;
            
            return player.CanAfford(cardData.manaCost);
        }
        
        private void PlayCardToSlot(int handIndex, int slotIndex)
        {
            Debug.Log($"[InputController] Playing card from hand {handIndex} to slot {slotIndex}");
            NetworkBoardState.Instance?.CmdPlayCard(LocalPlayerId, handIndex, slotIndex);
        }
        
        private void UseFlipAbilityOnCard(int handIndex, CardView targetCard)
        {
            Debug.Log($"[InputController] Using flip ability on card ({targetCard.BoardPosition.playerId}, {targetCard.BoardPosition.slotIndex})");
            NetworkBoardState.Instance?.CmdUseFlipAbility(
                LocalPlayerId, 
                handIndex, 
                targetCard.BoardPosition.playerId, 
                targetCard.BoardPosition.slotIndex
            );
        }
        
        #endregion
        
        #region Dragging Board Card
        
        private void HandleDraggingBoardCard()
        {
            // Draw targeting line
            if (_selectedCard != null && _targetingLine != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                
                _targetingLine.enabled = true;
                _targetingLine.SetPosition(0, _selectedCard.transform.position);
                _targetingLine.SetPosition(1, mousePos);
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                // Check if over a valid target
                var targetCard = GetCardUnderMouse();
                
                if (targetCard != null && targetCard.OwnerId != LocalPlayerId)
                {
                    // Attack enemy card
                    AttackCard(_selectedBoardSlot, targetCard);
                }
                else if (IsOverOpponentAvatar())
                {
                    // Attack opponent player directly
                    AttackPlayer(_selectedBoardSlot);
                }
                
                ClearSelection();
            }
        }
        
        private void HighlightValidAbilityTargets()
        {
            var state = NetworkBoardState.Instance?.GetState();
            if (state == null) return;
            
            // Highlight enemy cards (for offensive abilities)
            var enemy = state.GetOpponent(LocalPlayerId);
            var validTargets = new List<(int, int)>();
            
            for (int i = 0; i < 3; i++)
            {
                if (!enemy.IsSlotEmpty(i))
                {
                    validTargets.Add((1 - LocalPlayerId, i));
                }
            }
            
            _boardView?.HighlightSlots(validTargets, true);
        }
        
        private void HighlightValidSupportTargets()
        {
            var state = NetworkBoardState.Instance?.GetState();
            if (state == null) return;
            
            // Highlight friendly cards (for support abilities)
            var player = state.GetPlayer(LocalPlayerId);
            var validTargets = new List<(int, int)>();
            
            for (int i = 0; i < 3; i++)
            {
                if (!player.IsSlotEmpty(i))
                {
                    validTargets.Add((LocalPlayerId, i));
                }
            }
            
            _boardView?.HighlightSlots(validTargets, true);
        }
        
        private void AttackCard(int mySlotIndex, CardView targetCard)
        {
            Debug.Log($"[InputController] Attacking card ({targetCard.BoardPosition.playerId}, {targetCard.BoardPosition.slotIndex}) from slot {mySlotIndex}");
            NetworkBoardState.Instance?.CmdUseBoardAbility(
                LocalPlayerId,
                mySlotIndex,
                targetCard.BoardPosition.playerId,
                targetCard.BoardPosition.slotIndex
            );
        }
        
        private void UseSupportAbilityOnCard(int mySlotIndex, CardView targetCard)
        {
            Debug.Log($"[InputController] Using support ability on ({targetCard.BoardPosition.playerId}, {targetCard.BoardPosition.slotIndex}) from slot {mySlotIndex}");
            NetworkBoardState.Instance?.CmdUseSupportAbility(
                LocalPlayerId,
                mySlotIndex,
                targetCard.BoardPosition.playerId,
                targetCard.BoardPosition.slotIndex
            );
        }
        
        private void AttackPlayer(int mySlotIndex)
        {
            int opponentId = 1 - LocalPlayerId;
            Debug.Log($"[InputController] Attacking player {opponentId} from slot {mySlotIndex}");
            NetworkBoardState.Instance?.CmdAttackPlayer(LocalPlayerId, mySlotIndex, opponentId);
        }
        
        private void UseFlipAbilityOnPlayer(int handIndex)
        {
            int opponentId = 1 - LocalPlayerId;
            Debug.Log($"[InputController] Using flip ability on player {opponentId} from hand {handIndex}");
            NetworkBoardState.Instance?.CmdUseFlipAbilityOnPlayer(LocalPlayerId, handIndex, opponentId);
        }
        
        #endregion
        
        #region Target Selection
        
        private void HandleSelectingTarget()
        {
            // This state is for more complex targeting (not currently used)
            if (Input.GetMouseButtonUp(0))
            {
                ClearSelection();
            }
        }
        
        private void HandleSelectingSupportTarget()
        {
            // Draw targeting line (different color for support?)
            if (_selectedCard != null && _targetingLine != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                
                _targetingLine.enabled = true;
                _targetingLine.SetPosition(0, _selectedCard.transform.position);
                _targetingLine.SetPosition(1, mousePos);
            }
            
            // Left click to confirm target
            if (Input.GetMouseButtonDown(0))
            {
                var targetCard = GetCardUnderMouse();
                
                if (targetCard != null && targetCard.OwnerId == LocalPlayerId)
                {
                    // Use support ability on friendly card
                    UseSupportAbilityOnCard(_selectedBoardSlot, targetCard);
                }
                
                ClearSelection();
            }
            
            // Right click to cancel
            if (Input.GetMouseButtonDown(1))
            {
                ClearSelection();
            }
        }
        
        #endregion
        
        #region Helpers
        
        private CardView GetCardUnderMouse()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            // Try Physics2D raycast first
            var hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, _cardLayerMask);
            
            if (hit.collider != null)
            {
                var cardView = hit.collider.GetComponent<CardView>();
                if (cardView != null) return cardView;
                
                // Check parent
                cardView = hit.collider.GetComponentInParent<CardView>();
                if (cardView != null) return cardView;
            }
            
            // Fallback: Check all card views by position (for UI-based cards without colliders)
            // First check hand cards (from HandView)
            if (_myHandView != null)
            {
                foreach (var card in _myHandView.GetAllCards())
                {
                    if (card == null) continue;
                    
                    var rectTransform = card.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Camera.main))
                        {
                            return card;
                        }
                    }
                }
            }
            
            // Then check board cards (from BoardView)
            if (_boardView != null)
            {
                foreach (var kvp in _boardView.GetAllCardViews())
                {
                    var card = kvp.Value;
                    if (card == null) continue;
                    
                    // Check if mouse is within card bounds
                    var rectTransform = card.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Camera.main))
                        {
                            return card;
                        }
                    }
                }
            }
            
            return null;
        }
        
        private int GetSlotUnderMouse()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, _slotLayerMask);
            
            if (hit.collider != null)
            {
                // Try to determine slot index from collider name or component
                var slotName = hit.collider.gameObject.name;
                
                // Parse slot index from name (e.g., "Slot_0", "Slot_1", "Slot_2")
                if (slotName.Contains("0")) return 0;
                if (slotName.Contains("1")) return 1;
                if (slotName.Contains("2")) return 2;
            }
            
            return -1;
        }
        
        /// <summary>
        /// Check if the mouse is over the opponent's avatar zone.
        /// </summary>
        private bool IsOverOpponentAvatar()
        {
            // Method 1: Use LayerMask raycast
            if (_playerAvatarLayerMask != 0)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, _playerAvatarLayerMask);
                
                if (hit.collider != null)
                {
                    // Check if this is the opponent's avatar (not our own)
                    // Could use tag, name, or component check
                    return hit.collider.gameObject.name.ToLower().Contains("opponent") || 
                           hit.collider.gameObject.name.ToLower().Contains("enemy") ||
                           hit.collider.gameObject.name.ToLower().Contains("player1") ||
                           hit.collider == _boardView?.OpponentAvatarZone;
                }
            }
            
            // Method 2: Direct collider check from BoardView
            if (_boardView?.OpponentAvatarZone != null)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                return _boardView.OpponentAvatarZone.OverlapPoint(mousePos);
            }
            
            return false;
        }
        
        private void ClearSelection()
        {
            _state = InputState.Idle;
            _selectedCard = null;
            _selectedHandIndex = -1;
            _selectedBoardSlot = -1;
            _usingSupportAbility = false;
            
            if (_targetingLine != null)
            {
                _targetingLine.enabled = false;
            }
            
            _boardView?.ClearAllHighlights();
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Request to end the current turn.
        /// </summary>
        public void EndTurn()
        {
            if (!IsMyTurn())
            {
                Debug.Log("[InputController] Cannot end turn - not my turn");
                return;
            }
            
            Debug.Log("[InputController] Ending turn");
            NetworkBoardState.Instance?.CmdEndTurn(LocalPlayerId);
        }
        
        #endregion
    }
}
