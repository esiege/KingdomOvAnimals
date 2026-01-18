using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KOA.Network;
using KOA.Model;

namespace KOA.View
{
    /// <summary>
    /// UI component for turn management.
    /// Shows current turn indicator and end turn button.
    /// </summary>
    public class TurnUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _endTurnButton;
        [SerializeField] private TextMeshProUGUI _turnText;
        [SerializeField] private TextMeshProUGUI _turnNumberText;
        [SerializeField] private GameObject _yourTurnIndicator;
        [SerializeField] private GameObject _opponentTurnIndicator;
        [SerializeField] private InputController _inputController;
        
        [Header("Colors")]
        [SerializeField] private Color _myTurnColor = Color.green;
        [SerializeField] private Color _opponentTurnColor = Color.red;
        
        private int _localPlayerId;
        
        private void Start()
        {
            // Hook up button
            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }
            
            // Subscribe to turn changes
            if (NetworkBoardState.Instance != null)
            {
                NetworkBoardState.Instance.OnTurnChanged += OnTurnChanged;
                NetworkBoardState.Instance.OnStateChanged += OnStateChanged;
            }
            
            // Get local player ID from BoardView
            var boardView = FindObjectOfType<BoardView>();
            if (boardView != null)
            {
                _localPlayerId = boardView.LocalPlayerId;
            }
            
            // Initial update
            UpdateUI(NetworkBoardState.Instance?.GetState());
        }
        
        private void OnDestroy()
        {
            if (NetworkBoardState.Instance != null)
            {
                NetworkBoardState.Instance.OnTurnChanged -= OnTurnChanged;
                NetworkBoardState.Instance.OnStateChanged -= OnStateChanged;
            }
            
            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
            }
        }
        
        private void OnEndTurnClicked()
        {
            if (_inputController != null)
            {
                _inputController.EndTurn();
            }
            else
            {
                // Direct call if no input controller
                NetworkBoardState.Instance?.CmdEndTurn(_localPlayerId);
            }
        }
        
        private void OnTurnChanged(int previousPlayerId, int newActivePlayerId)
        {
            UpdateTurnIndicators(newActivePlayerId);
        }
        
        private void OnStateChanged(BoardState state)
        {
            UpdateUI(state);
        }
        
        private void UpdateUI(BoardState state)
        {
            if (state == null) return;
            
            UpdateTurnIndicators(state.CurrentTurnPlayerId);
            
            if (_turnNumberText != null)
            {
                _turnNumberText.text = $"Turn {state.TurnNumber}";
            }
        }
        
        private void UpdateTurnIndicators(int activePlayerId)
        {
            bool isMyTurn = activePlayerId == _localPlayerId;
            
            // Update button interactability
            if (_endTurnButton != null)
            {
                _endTurnButton.interactable = isMyTurn;
            }
            
            // Update turn text
            if (_turnText != null)
            {
                _turnText.text = isMyTurn ? "Your Turn" : "Opponent's Turn";
                _turnText.color = isMyTurn ? _myTurnColor : _opponentTurnColor;
            }
            
            // Update indicators
            if (_yourTurnIndicator != null)
            {
                _yourTurnIndicator.SetActive(isMyTurn);
            }
            
            if (_opponentTurnIndicator != null)
            {
                _opponentTurnIndicator.SetActive(!isMyTurn);
            }
        }
    }
}
