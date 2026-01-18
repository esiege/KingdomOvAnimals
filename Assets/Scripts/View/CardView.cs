using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KOA.Model;
using KOA.Data;

namespace KOA.View
{
    /// <summary>
    /// Visual representation of a card.
    /// Renders CardState data - has no game logic.
    /// </summary>
    public class CardView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _manaText;
        [SerializeField] private TextMeshProUGUI _attackText;
        [SerializeField] private TextMeshProUGUI _defenseText;
        [SerializeField] private Image _cardImage;
        [SerializeField] private Image _cardBackground;
        [SerializeField] private GameObject _highlightOverlay;
        [SerializeField] private GameObject _tappedOverlay;
        [SerializeField] private GameObject _summoningSicknessIcon;
        
        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _damagedColor = new Color(1f, 0.5f, 0.5f);
        [SerializeField] private Color _highlightColor = Color.yellow;
        
        /// <summary>
        /// The instance ID of the card this view represents.
        /// </summary>
        public int InstanceId { get; private set; }
        
        /// <summary>
        /// The CardData ID for looking up abilities.
        /// </summary>
        public string CardDataId { get; private set; }
        
        /// <summary>
        /// Owner player ID.
        /// </summary>
        public int OwnerId { get; private set; }
        
        /// <summary>
        /// Current board position (playerId, slotIndex) or (-1, -1) if in hand.
        /// </summary>
        public (int playerId, int slotIndex) BoardPosition { get; private set; } = (-1, -1);
        
        /// <summary>
        /// Is this card in a player's hand?
        /// </summary>
        public bool IsInHand => BoardPosition.playerId < 0;
        
        /// <summary>
        /// Hand index if in hand, -1 otherwise.
        /// </summary>
        public int HandIndex { get; set; } = -1;
        
        private CardData _cardData;
        private int _maxHealth;
        private bool _isHighlighted;
        
        /// <summary>
        /// Initialize this view with a CardState.
        /// </summary>
        public void Initialize(CardState state, CardLibrary cardLibrary)
        {
            if (state == null || !state.IsValid)
            {
                Debug.LogWarning("[CardView] Attempting to initialize with invalid state");
                return;
            }
            
            InstanceId = state.InstanceId;
            CardDataId = state.CardDataId;
            OwnerId = state.OwnerId;
            
            _cardData = cardLibrary.GetCardById(state.CardDataId);
            if (_cardData == null)
            {
                Debug.LogError($"[CardView] Could not find CardData for {state.CardDataId}");
                return;
            }
            
            _maxHealth = _cardData.health;
            
            // Set static data from CardData
            if (_nameText != null) _nameText.text = _cardData.displayName;
            if (_manaText != null) _manaText.text = _cardData.manaCost.ToString();
            if (_cardImage != null && _cardData.artwork != null)
            {
                _cardImage.sprite = _cardData.artwork;
            }
            
            // Set attack/defense from abilities
            if (_attackText != null)
            {
                int attack = _cardData.offensiveAbility != null ? _cardData.offensiveAbility.damage : 0;
                _attackText.text = attack.ToString();
            }
            if (_defenseText != null)
            {
                int defense = _cardData.defensiveAbility != null ? _cardData.defensiveAbility.damage : 0;
                _defenseText.text = defense.ToString();
            }
            
            // Update with current state
            UpdateFromState(state);
            
            Debug.Log($"[CardView] Initialized: {_cardData.displayName} (Instance #{InstanceId})");
        }
        
        /// <summary>
        /// Update view from CardState changes.
        /// </summary>
        public void UpdateFromState(CardState state)
        {
            if (state == null || state.InstanceId != InstanceId)
            {
                Debug.LogWarning("[CardView] State mismatch in UpdateFromState");
                return;
            }
            
            // Update health
            if (_healthText != null)
            {
                _healthText.text = state.CurrentHealth.ToString();
                _healthText.color = state.CurrentHealth < _maxHealth ? _damagedColor : _normalColor;
            }
            
            // Update status visuals
            if (_tappedOverlay != null) _tappedOverlay.SetActive(state.IsTapped);
            if (_summoningSicknessIcon != null) _summoningSicknessIcon.SetActive(state.HasSummoningSickness);
            
            // Handle flipped state
            // TODO: Implement flip animation
        }
        
        /// <summary>
        /// Set board position.
        /// </summary>
        public void SetBoardPosition(int playerId, int slotIndex)
        {
            BoardPosition = (playerId, slotIndex);
            HandIndex = -1;
        }
        
        /// <summary>
        /// Set as hand card.
        /// </summary>
        public void SetHandPosition(int handIndex)
        {
            BoardPosition = (-1, -1);
            HandIndex = handIndex;
        }
        
        /// <summary>
        /// Highlight this card (for targeting/selection).
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            _isHighlighted = highlighted;
            if (_highlightOverlay != null) _highlightOverlay.SetActive(highlighted);
            if (_cardBackground != null)
            {
                _cardBackground.color = highlighted ? _highlightColor : _normalColor;
            }
        }
        
        /// <summary>
        /// Show card back (for opponent's hand).
        /// </summary>
        public void SetFaceDown(bool faceDown)
        {
            // Hide card content when face down
            if (_nameText != null) _nameText.gameObject.SetActive(!faceDown);
            if (_healthText != null) _healthText.gameObject.SetActive(!faceDown);
            if (_manaText != null) _manaText.gameObject.SetActive(!faceDown);
            if (_attackText != null) _attackText.gameObject.SetActive(!faceDown);
            if (_defenseText != null) _defenseText.gameObject.SetActive(!faceDown);
            if (_cardImage != null) _cardImage.gameObject.SetActive(!faceDown);
        }
        
        /// <summary>
        /// Play damage animation.
        /// </summary>
        public void PlayDamageAnimation(int damage)
        {
            // TODO: Implement damage VFX
            Debug.Log($"[CardView] Damage animation: {damage}");
        }
        
        /// <summary>
        /// Play death animation and destroy.
        /// </summary>
        public void PlayDeathAnimation(System.Action onComplete = null)
        {
            // TODO: Implement death animation
            Debug.Log($"[CardView] Death animation");
            
            // For now, just destroy immediately
            if (onComplete != null) onComplete();
            Destroy(gameObject, 0.1f);
        }
        
        /// <summary>
        /// Get CardData for this card.
        /// </summary>
        public CardData GetCardData() => _cardData;
    }
}
