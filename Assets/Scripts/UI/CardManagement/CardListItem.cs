using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KOA.Data;

namespace KOA.UI
{
    /// <summary>
    /// Individual item in the card list. Displays card name and handles selection.
    /// </summary>
    public class CardListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image backgroundImage;
        
        [Header("Selection Colors")]
        [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.4f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.4f, 0.5f, 0.6f, 1f);
        [SerializeField] private Color hoverColor = new Color(0.35f, 0.35f, 0.45f, 1f);
        
        public CardData Card { get; private set; }
        
        private System.Action<CardData> onSelected;
        private bool isSelected;

        private void Awake()
        {
            // Auto-find components if not assigned
            if (nameText == null)
                nameText = GetComponentInChildren<TMP_Text>();
            if (selectButton == null)
                selectButton = GetComponent<Button>();
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
        }

        public void Initialize(CardData card, System.Action<CardData> onSelectedCallback)
        {
            Card = card;
            onSelected = onSelectedCallback;
            
            if (nameText != null)
                nameText.text = card.displayName;
            
            if (selectButton != null)
                selectButton.onClick.AddListener(OnClick);
            
            SetSelected(false);
        }

        private void OnClick()
        {
            onSelected?.Invoke(Card);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            
            if (backgroundImage != null)
            {
                backgroundImage.color = selected ? selectedColor : normalColor;
            }
            
            // Update button colors for hover state
            if (selectButton != null)
            {
                var colors = selectButton.colors;
                colors.normalColor = selected ? selectedColor : normalColor;
                colors.highlightedColor = hoverColor;
                colors.selectedColor = selectedColor;
                selectButton.colors = colors;
            }
        }

        private void OnDestroy()
        {
            if (selectButton != null)
                selectButton.onClick.RemoveAllListeners();
        }
    }
}
