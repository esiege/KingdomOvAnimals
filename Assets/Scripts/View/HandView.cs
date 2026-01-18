using UnityEngine;
using System.Collections.Generic;
using KOA.Model;
using KOA.Network;
using KOA.Data;

namespace KOA.View
{
    /// <summary>
    /// Manages the layout and display of cards in a player's hand.
    /// Handles card positioning, fanning, and hover effects.
    /// </summary>
    public class HandView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _handContainer;
        [SerializeField] private CardLibrary _cardLibrary;
        [SerializeField] private BoardView _boardView;
        
        [Header("Layout")]
        [SerializeField] private float _cardSpacing = 1.2f;
        [SerializeField] private float _cardFanAngle = 5f;
        [SerializeField] private float _cardArcHeight = 0.5f;
        [SerializeField] private float _hoverLift = 0.5f;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject _cardPrefab;
        
        private List<CardView> _handCards = new List<CardView>();
        private CardView _hoveredCard;
        private int _playerId;
        
        /// <summary>
        /// Player ID this hand belongs to.
        /// </summary>
        public int PlayerId
        {
            get => _playerId;
            set => _playerId = value;
        }
        
        /// <summary>
        /// Whether this is the local player's hand (shows card faces).
        /// </summary>
        public bool IsLocalHand { get; set; } = true;
        
        private void Start()
        {
            if (_cardLibrary == null)
            {
                _cardLibrary = CardLibrary.Instance;
            }
            
            if (_boardView == null)
            {
                _boardView = FindObjectOfType<BoardView>();
            }
            
            // Subscribe to state changes
            if (NetworkBoardState.Instance != null)
            {
                NetworkBoardState.Instance.OnStateChanged += OnStateChanged;
            }
        }
        
        private void OnDestroy()
        {
            if (NetworkBoardState.Instance != null)
            {
                NetworkBoardState.Instance.OnStateChanged -= OnStateChanged;
            }
        }
        
        private void OnStateChanged(BoardState state)
        {
            RenderHand(state);
        }
        
        /// <summary>
        /// Render the hand for the specified player.
        /// </summary>
        public void RenderHand(BoardState state)
        {
            if (state == null) return;
            
            var player = state.GetPlayer(_playerId);
            if (player == null) return;
            
            var hand = player.Hand;
            
            // Ensure we have the right number of card views
            while (_handCards.Count < hand.Count)
            {
                CreateHandCardView();
            }
            
            while (_handCards.Count > hand.Count)
            {
                DestroyLastHandCard();
            }
            
            // Update each card view
            for (int i = 0; i < hand.Count; i++)
            {
                var cardState = hand[i];
                var cardView = _handCards[i];
                
                if (cardState != null && cardState.IsValid && cardView != null)
                {
                    cardView.Initialize(cardState, _cardLibrary);
                    cardView.HandIndex = i;
                    
                    // Show card back for opponent
                    if (!IsLocalHand)
                    {
                        cardView.SetFaceDown(true);
                    }
                    
                    // Position card
                    PositionHandCard(cardView, i, hand.Count);
                }
            }
        }
        
        private void CreateHandCardView()
        {
            if (_cardPrefab == null || _handContainer == null)
            {
                Debug.LogWarning("[HandView] Missing card prefab or hand container");
                return;
            }
            
            var cardObj = Instantiate(_cardPrefab, _handContainer);
            var cardView = cardObj.GetComponent<CardView>();
            
            if (cardView != null)
            {
                _handCards.Add(cardView);
            }
        }
        
        private void DestroyLastHandCard()
        {
            if (_handCards.Count > 0)
            {
                var last = _handCards[_handCards.Count - 1];
                _handCards.RemoveAt(_handCards.Count - 1);
                
                if (last != null)
                {
                    Destroy(last.gameObject);
                }
            }
        }
        
        private void PositionHandCard(CardView cardView, int index, int totalCards)
        {
            if (cardView == null) return;
            
            // Calculate position in fan
            float centerIndex = (totalCards - 1) / 2f;
            float offset = index - centerIndex;
            
            // X position (spread)
            float x = offset * _cardSpacing;
            
            // Y position (arc)
            float normalizedOffset = totalCards > 1 ? offset / centerIndex : 0;
            float y = -Mathf.Abs(normalizedOffset) * _cardArcHeight;
            
            // Rotation (fan)
            float rotation = -offset * _cardFanAngle;
            
            // Apply position
            cardView.transform.localPosition = new Vector3(x, y, -index * 0.01f);
            cardView.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            
            // Scale down slightly if many cards
            float scale = totalCards > 5 ? 0.9f : 1f;
            cardView.transform.localScale = Vector3.one * scale;
        }
        
        /// <summary>
        /// Handle card hover (called by InputController or EventTrigger).
        /// </summary>
        public void OnCardHover(CardView card)
        {
            if (_hoveredCard == card) return;
            
            // Unhover previous
            if (_hoveredCard != null)
            {
                OnCardUnhover(_hoveredCard);
            }
            
            _hoveredCard = card;
            
            if (card != null && IsLocalHand)
            {
                // Lift card
                var pos = card.transform.localPosition;
                pos.y += _hoverLift;
                card.transform.localPosition = pos;
                
                // Bring to front
                card.transform.SetAsLastSibling();
            }
        }
        
        /// <summary>
        /// Handle card unhover.
        /// </summary>
        public void OnCardUnhover(CardView card)
        {
            if (card != null && _handCards.Contains(card))
            {
                // Reposition card back to its correct position
                int index = _handCards.IndexOf(card);
                PositionHandCard(card, index, _handCards.Count);
            }
            
            if (_hoveredCard == card)
            {
                _hoveredCard = null;
            }
        }
        
        /// <summary>
        /// Get card at hand index.
        /// </summary>
        public CardView GetCardAtIndex(int index)
        {
            if (index >= 0 && index < _handCards.Count)
            {
                return _handCards[index];
            }
            return null;
        }
        
        /// <summary>
        /// Clear all cards from hand.
        /// </summary>
        public void Clear()
        {
            foreach (var card in _handCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            _handCards.Clear();
        }
    }
}
