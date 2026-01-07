using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KOA.Data;

namespace KOA.UI
{
    /// <summary>
    /// Individual item in the ability list. Displays ability name and handles selection.
    /// </summary>
    public class AbilityListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image backgroundImage;
        
        [Header("Selection Colors")]
        [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.4f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.4f, 0.5f, 0.6f, 1f);
        [SerializeField] private Color hoverColor = new Color(0.35f, 0.35f, 0.45f, 1f);
        
        public AbilityData Ability { get; private set; }
        
        private System.Action<AbilityData> onSelected;
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

        public void Initialize(AbilityData ability, System.Action<AbilityData> onSelectedCallback)
        {
            Ability = ability;
            onSelected = onSelectedCallback;
            
            if (nameText != null)
                nameText.text = ability.displayName;
            
            if (selectButton != null)
                selectButton.onClick.AddListener(OnClick);
            
            SetSelected(false);
        }

        private void OnClick()
        {
            onSelected?.Invoke(Ability);
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
