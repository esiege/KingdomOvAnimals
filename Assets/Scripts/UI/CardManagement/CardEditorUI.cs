using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KOA.Data;

namespace KOA.UI
{
    /// <summary>
    /// UI for editing CardData assets in the Card Management scene.
    /// Provides list view, search, and editing capabilities.
    /// </summary>
    public class CardEditorUI : MonoBehaviour
    {
        [Header("List Panel")]
        [SerializeField] private Transform cardListContent;
        [SerializeField] private GameObject cardListItemPrefab;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private Button newCardButton;

        [Header("Editor Panel")]
        [SerializeField] private GameObject editorPanel;
        [SerializeField] private TMP_InputField idField;
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private TMP_InputField healthField;
        [SerializeField] private TMP_InputField manaCostField;
        [SerializeField] private TMP_InputField descriptionField;
        [SerializeField] private TMP_Dropdown offensiveDropdown;
        [SerializeField] private TMP_Dropdown defensiveDropdown;
        [SerializeField] private Image artworkImage;
        [SerializeField] private Button selectArtworkButton;

        [Header("Editor Buttons")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button deleteButton;

        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmDialog;
        [SerializeField] private TMP_Text confirmText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        private CardData selectedCard;
        private List<CardData> allCards = new List<CardData>();
        private List<AbilityData> allAbilities = new List<AbilityData>();
        private List<CardListItem> listItems = new List<CardListItem>();

        private void Start()
        {
            SetupButtonListeners();
            SetupDropdowns();
            LoadAllData();
            RefreshCardList();
            
            // Hide editor panel until a card is selected
            if (editorPanel != null)
                editorPanel.SetActive(false);
            
            // Hide confirmation dialog
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
        }

        private void SetupButtonListeners()
        {
            if (searchField != null)
                searchField.onValueChanged.AddListener(OnSearchChanged);
            
            if (newCardButton != null)
                newCardButton.onClick.AddListener(CreateNewCard);
            
            if (saveButton != null)
                saveButton.onClick.AddListener(SaveCard);
            
            if (deleteButton != null)
                deleteButton.onClick.AddListener(PromptDeleteCard);
            
            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(ConfirmDelete);
            
            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(CancelDelete);
            
            if (selectArtworkButton != null)
                selectArtworkButton.onClick.AddListener(SelectArtwork);
        }

        private void SetupDropdowns()
        {
            // No card type dropdown anymore
        }

        private void LoadAllData()
        {
            // Load all cards from Resources
            allCards = Resources.LoadAll<CardData>("Cards").ToList();
            Debug.Log($"[CardEditor] Loaded {allCards.Count} cards");
            
            // Load all abilities for dropdowns
            allAbilities = Resources.LoadAll<AbilityData>("Abilities").ToList();
            Debug.Log($"[CardEditor] Loaded {allAbilities.Count} abilities");
            
            PopulateAbilityDropdowns();
        }

        private void PopulateAbilityDropdowns()
        {
            var abilityOptions = new List<string> { "(None)" };
            abilityOptions.AddRange(allAbilities.Select(a => a.displayName));
            
            if (offensiveDropdown != null)
            {
                offensiveDropdown.ClearOptions();
                offensiveDropdown.AddOptions(abilityOptions);
            }
            
            if (defensiveDropdown != null)
            {
                defensiveDropdown.ClearOptions();
                defensiveDropdown.AddOptions(abilityOptions);
            }
        }

        #region Card List

        public void RefreshCardList()
        {
            // Clear existing items
            foreach (var item in listItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            listItems.Clear();
            
            // Reload cards in case new ones were created
            allCards = Resources.LoadAll<CardData>("Cards").ToList();
            
            // Filter by search
            string filter = searchField != null ? searchField.text.ToLower() : "";
            var filteredCards = string.IsNullOrEmpty(filter) 
                ? allCards 
                : allCards.Where(c => c.displayName.ToLower().Contains(filter) || c.id.ToLower().Contains(filter)).ToList();
            
            // Create list items
            foreach (var card in filteredCards)
            {
                CreateListItem(card);
            }
            
            Debug.Log($"[CardEditor] Displaying {filteredCards.Count} cards");
        }

        private void CreateListItem(CardData card)
        {
            if (cardListItemPrefab == null)
            {
                Debug.LogError("[CardEditor] cardListItemPrefab is null! Trying to load from Resources...");
                cardListItemPrefab = Resources.Load<GameObject>("UI/CardListItem");
                if (cardListItemPrefab == null)
                {
                    Debug.LogError("[CardEditor] Failed to load CardListItem prefab from Resources/UI/");
                    return;
                }
            }
            
            if (cardListContent == null)
            {
                Debug.LogError("[CardEditor] cardListContent is null!");
                return;
            }
            
            var itemObj = Instantiate(cardListItemPrefab, cardListContent);
            var listItem = itemObj.GetComponent<CardListItem>();
            
            if (listItem != null)
            {
                listItem.Initialize(card, OnCardSelected);
                listItems.Add(listItem);
            }
        }

        private void OnSearchChanged(string value)
        {
            RefreshCardList();
        }

        private void OnCardSelected(CardData card)
        {
            SelectCard(card);
        }

        #endregion

        #region Card Selection & Editing

        public void SelectCard(CardData card)
        {
            selectedCard = card;
            
            if (editorPanel != null)
                editorPanel.SetActive(true);
            
            // Populate fields
            if (idField != null) idField.text = card.id;
            if (nameField != null) nameField.text = card.displayName;
            if (healthField != null) healthField.text = card.health.ToString();
            if (manaCostField != null) manaCostField.text = card.manaCost.ToString();
            if (descriptionField != null) descriptionField.text = card.description ?? "";
            
            // Set artwork image
            if (artworkImage != null)
                artworkImage.sprite = card.artwork;
            
            // Set ability dropdowns
            SetAbilityDropdown(offensiveDropdown, card.offensiveAbility);
            SetAbilityDropdown(defensiveDropdown, card.defensiveAbility);
            
            // Update list selection visual
            foreach (var item in listItems)
            {
                item.SetSelected(item.Card == card);
            }
            
            Debug.Log($"[CardEditor] Selected card: {card.displayName}");
        }

        private void SetAbilityDropdown(TMP_Dropdown dropdown, AbilityData ability)
        {
            if (dropdown == null) return;
            
            if (ability == null)
            {
                dropdown.value = 0; // (None)
            }
            else
            {
                int index = allAbilities.FindIndex(a => a == ability);
                dropdown.value = index + 1; // +1 because of (None) option
            }
        }

        private AbilityData GetAbilityFromDropdown(TMP_Dropdown dropdown)
        {
            if (dropdown == null || dropdown.value == 0)
                return null;
            
            int abilityIndex = dropdown.value - 1; // -1 because of (None) option
            if (abilityIndex >= 0 && abilityIndex < allAbilities.Count)
                return allAbilities[abilityIndex];
            
            return null;
        }

        #endregion

        #region Save/Create/Delete

        public void SaveCard()
        {
            if (selectedCard == null) return;
            
            // Update card data
            selectedCard.id = idField != null ? idField.text : selectedCard.id;
            selectedCard.displayName = nameField != null ? nameField.text : selectedCard.displayName;
            selectedCard.health = healthField != null ? int.Parse(healthField.text) : selectedCard.health;
            selectedCard.manaCost = manaCostField != null ? int.Parse(manaCostField.text) : selectedCard.manaCost;
            selectedCard.description = descriptionField != null ? descriptionField.text : selectedCard.description;
            
            selectedCard.offensiveAbility = GetAbilityFromDropdown(offensiveDropdown);
            selectedCard.defensiveAbility = GetAbilityFromDropdown(defensiveDropdown);
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(selectedCard);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"[CardEditor] Saved card: {selectedCard.displayName}");
            #endif
            
            RefreshCardList();
            SelectCard(selectedCard);
        }

        public void SelectArtwork()
        {
            #if UNITY_EDITOR
            if (selectedCard == null) return;
            
            string path = UnityEditor.EditorUtility.OpenFilePanel("Select Artwork", "Assets", "png,jpg,jpeg");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert to relative path
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                    
                    // Load sprite from path
                    var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null)
                    {
                        selectedCard.artwork = sprite;
                        if (artworkImage != null)
                            artworkImage.sprite = sprite;
                        
                        UnityEditor.EditorUtility.SetDirty(selectedCard);
                        Debug.Log($"[CardEditor] Set artwork: {path}");
                    }
                    else
                    {
                        Debug.LogWarning($"[CardEditor] Could not load sprite from: {path}. Make sure texture import settings have 'Sprite Mode' enabled.");
                    }
                }
                else
                {
                    Debug.LogWarning("[CardEditor] Please select an image from within the Assets folder");
                }
            }
            #endif
        }

        public void CreateNewCard()
        {
            #if UNITY_EDITOR
            var newCard = ScriptableObject.CreateInstance<CardData>();
            newCard.id = "new_card_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            newCard.displayName = "New Card";
            newCard.health = 1;
            newCard.manaCost = 1;
            
            // Ensure directory exists
            if (!System.IO.Directory.Exists("Assets/Resources/Cards"))
            {
                System.IO.Directory.CreateDirectory("Assets/Resources/Cards");
            }
            
            string path = "Assets/Resources/Cards/" + newCard.id + ".asset";
            UnityEditor.AssetDatabase.CreateAsset(newCard, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            Debug.Log($"[CardEditor] Created new card: {path}");
            
            RefreshCardList();
            SelectCard(newCard);
            #else
            Debug.LogWarning("[CardEditor] Cannot create cards outside of Editor");
            #endif
        }

        public void PromptDeleteCard()
        {
            if (selectedCard == null) return;
            
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(true);
                if (confirmText != null)
                    confirmText.text = $"Delete '{selectedCard.displayName}'?\nThis cannot be undone.";
            }
        }

        private void ConfirmDelete()
        {
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
            
            DeleteSelectedCard();
        }

        private void CancelDelete()
        {
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
        }

        private void DeleteSelectedCard()
        {
            if (selectedCard == null) return;
            
            #if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(selectedCard);
            if (!string.IsNullOrEmpty(path))
            {
                Debug.Log($"[CardEditor] Deleting card: {path}");
                UnityEditor.AssetDatabase.DeleteAsset(path);
                UnityEditor.AssetDatabase.Refresh();
            }
            
            selectedCard = null;
            if (editorPanel != null)
                editorPanel.SetActive(false);
            
            RefreshCardList();
            #else
            Debug.LogWarning("[CardEditor] Cannot delete cards outside of Editor");
            #endif
        }

        #endregion
    }
}
