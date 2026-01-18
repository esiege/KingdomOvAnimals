using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KOA.Model;
using KOA.Network;

namespace KOA.View
{
    /// <summary>
    /// Displays a player's health and mana.
    /// </summary>
    public class PlayerStatsView : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int _playerId = 0;
        [SerializeField] private bool _useLocalPerspective = false;
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _manaText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _healthFill;
        [SerializeField] private Image _manaFill;
        
        /// <summary>
        /// Player ID this view displays (0 or 1).
        /// If useLocalPerspective is true, 0 means "local player" and 1 means "opponent".
        /// </summary>
        public int PlayerId
        {
            get => _playerId;
            set => _playerId = value;
        }
        
        /// <summary>
        /// Get the actual player ID to display, accounting for perspective.
        /// </summary>
        private int GetActualPlayerId()
        {
            if (!_useLocalPerspective) return _playerId;
            
            // 0 = local, 1 = opponent
            var boardView = FindObjectOfType<BoardView>();
            if (boardView == null) return _playerId;
            
            return _playerId == 0 ? boardView.LocalPlayerId : (1 - boardView.LocalPlayerId);
        }
        
        private void Start()
        {
            if (NetworkBoardState.Instance != null)
            {
                NetworkBoardState.Instance.OnStateChanged += OnStateChanged;
                NetworkBoardState.Instance.OnPlayerDamaged += OnPlayerDamaged;
                
                // Initial update
                var state = NetworkBoardState.Instance.GetState();
                if (state != null)
                {
                    UpdateFromState(state.GetPlayer(GetActualPlayerId()));
                }
            }
        }
        
        private void OnDestroy()
        {
            if (NetworkBoardState.Instance != null)
            {
                NetworkBoardState.Instance.OnStateChanged -= OnStateChanged;
                NetworkBoardState.Instance.OnPlayerDamaged -= OnPlayerDamaged;
            }
        }
        
        private void OnStateChanged(BoardState state)
        {
            var player = state.GetPlayer(GetActualPlayerId());
            if (player != null)
            {
                UpdateFromState(player);
            }
        }
        
        private void OnPlayerDamaged(int playerId, int damage, int newHealth)
        {
            if (playerId == GetActualPlayerId())
            {
                // TODO: Play damage animation
                Debug.Log($"[PlayerStatsView] Player {playerId} took {damage} damage");
            }
        }
        
        /// <summary>
        /// Update display from player state.
        /// </summary>
        public void UpdateFromState(PlayerBoardState state)
        {
            if (state == null) return;
            
            if (_healthText != null)
            {
                _healthText.text = $"{state.Health}/{state.MaxHealth}";
            }
            
            if (_manaText != null)
            {
                _manaText.text = $"{state.Mana}/{state.MaxMana}";
            }
            
            if (_healthFill != null)
            {
                _healthFill.fillAmount = (float)state.Health / state.MaxHealth;
            }
            
            if (_manaFill != null)
            {
                _manaFill.fillAmount = (float)state.Mana / state.MaxMana;
            }
        }
        
        /// <summary>
        /// Set player name.
        /// </summary>
        public void SetPlayerName(string name)
        {
            if (_nameText != null)
            {
                _nameText.text = name;
            }
        }
    }
}
