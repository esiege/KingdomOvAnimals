using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KOA.Data;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KOA.UI
{
    /// <summary>
    /// UI controller for editing DeckData assets.
    /// Uses a deck list panel instead of dropdown for deck selection.
    /// </summary>
    public class DeckEditorUI : MonoBehaviour
    {
        [Header("Deck List")]
        [SerializeField] private Transform deckListContent;
        [SerializeField] private GameObject deckListItemPrefab;
        [SerializeField] private Button newDeckButton;
        [SerializeField] private Button deleteDeckButton;
        
        [Header("Available Cards")]
        [SerializeField] private Transform availableCardsContent;
        [SerializeField] private GameObject availableCardItemPrefab;
        
        [Header("Deck Contents")]
        [SerializeField] private Transform deckContentsContent;
        [SerializeField] private TMP_Text deckTitleText;
        [SerializeField] private GameObject deckCardItemPrefab;
        
        [Header("Stats & Actions")]
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private Button saveButton;
        
        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmDialog;
        [SerializeField] private TMP_Text confirmText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        
        private List<DeckData> allDecks = new List<DeckData>();
        private List<CardData> allCards = new List<CardData>();
        private DeckData currentDeck;
        private List<CardData> workingDeckCards = new List<CardData>();
        private bool hasUnsavedChanges;
        private int selectedDeckIndex = -1;
        
        private System.Action pendingConfirmAction;
        
        // Colors for selection
        private readonly Color normalColor = new Color(0.3f, 0.3f, 0.4f, 1f);
        private readonly Color selectedColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        
        private void Start()
        {
            LoadAllDecks();
            LoadAllCards();
            SetupUI();
            RefreshDeckList();
            
            if (allDecks.Count > 0)
            {
                SelectDeck(0);
            }
            else
            {
                ClearEditor();
            }
        }
        
        private void LoadAllDecks()
        {
            allDecks = Resources.LoadAll<DeckData>("Decks").ToList();
            allDecks.Sort((a, b) => string.Compare(a.deckName, b.deckName));
            Debug.Log($"[DeckEditor] Loaded {allDecks.Count} decks");
        }
        
        private void LoadAllCards()
        {
            allCards = Resources.LoadAll<CardData>("Cards").ToList();
            allCards.Sort((a, b) => string.Compare(a.displayName, b.displayName));
            Debug.Log($"[DeckEditor] Loaded {allCards.Count} cards");
        }
        
        private void SetupUI()
        {
            // Buttons
            if (newDeckButton != null)
            {
                newDeckButton.onClick.AddListener(OnNewDeckClicked);
            }
            
            if (deleteDeckButton != null)
            {
                deleteDeckButton.onClick.AddListener(OnDeleteDeckClicked);
            }
            
            if (saveButton != null)
            {
                saveButton.onClick.AddListener(OnSaveClicked);
            }
            
            // Confirm dialog
            if (confirmYesButton != null)
            {
                confirmYesButton.onClick.AddListener(OnConfirmYes);
            }
            
            if (confirmNoButton != null)
            {
                confirmNoButton.onClick.AddListener(OnConfirmNo);
            }
            
            HideConfirmDialog();
        }
        
        private void RefreshDeckList()
        {
            ClearContent(deckListContent);
            
            if (deckListItemPrefab == null || deckListContent == null) return;
            
            for (int i = 0; i < allDecks.Count; i++)
            {
                var deck = allDecks[i];
                if (deck == null) continue;
                
                int index = i; // Capture for closure
                GameObject item = Instantiate(deckListItemPrefab, deckListContent);
                SetupDeckListItem(item, deck, index);
            }
        }
        
        private void SetupDeckListItem(GameObject item, DeckData deck, int index)
        {
            // Name
            var nameText = item.transform.Find("NameText")?.GetComponent<TMP_Text>();
            if (nameText != null)
            {
                nameText.text = deck.deckName;
            }
            
            // Card count
            var countText = item.transform.Find("CountText")?.GetComponent<TMP_Text>();
            if (countText != null)
            {
                int cardCount = deck.cards != null ? deck.cards.Count : 0;
                countText.text = $"{cardCount} cards";
            }
            
            // Background for selection state
            var bg = item.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = (index == selectedDeckIndex) ? selectedColor : normalColor;
            }
            
            // Click handler
            var button = item.GetComponent<Button>();
            if (button == null)
            {
                button = item.AddComponent<Button>();
            }
            button.onClick.AddListener(() => OnDeckItemClicked(index));
        }
        
        private void OnDeckItemClicked(int index)
        {
            if (hasUnsavedChanges)
            {
                ShowConfirmDialog("Discard unsaved changes?", () =>
                {
                    SelectDeck(index);
                });
            }
            else
            {
                SelectDeck(index);
            }
        }
        
        private void SelectDeck(int index)
        {
            if (index < 0 || index >= allDecks.Count)
            {
                ClearEditor();
                return;
            }
            
            selectedDeckIndex = index;
            currentDeck = allDecks[index];
            
            // Copy deck cards to working list
            workingDeckCards.Clear();
            if (currentDeck.cards != null)
            {
                workingDeckCards.AddRange(currentDeck.cards.Where(c => c != null));
            }
            
            hasUnsavedChanges = false;
            
            RefreshDeckList(); // Update selection highlighting
            RefreshAvailableCards();
            RefreshDeckContents();
            UpdateStats();
            
            Debug.Log($"[DeckEditor] Selected deck: {currentDeck.deckName}");
        }
        
        private void ClearEditor()
        {
            currentDeck = null;
            selectedDeckIndex = -1;
            workingDeckCards.Clear();
            hasUnsavedChanges = false;
            
            ClearContent(availableCardsContent);
            ClearContent(deckContentsContent);
            
            if (deckTitleText != null)
            {
                deckTitleText.text = "Deck Contents (0/30)";
            }
            
            if (statsText != null)
            {
                statsText.text = "No deck selected";
            }
        }
        
        private void RefreshAvailableCards()
        {
            ClearContent(availableCardsContent);
            
            if (availableCardItemPrefab == null || availableCardsContent == null) return;
            
            foreach (var card in allCards)
            {
                if (card == null) continue;
                
                GameObject item = Instantiate(availableCardItemPrefab, availableCardsContent);
                SetupAvailableCardItem(item, card);
            }
        }
        
        private void SetupAvailableCardItem(GameObject item, CardData card)
        {
            // Name
            var nameText = item.transform.Find("NameText")?.GetComponent<TMP_Text>();
            if (nameText != null)
            {
                nameText.text = card.displayName;
            }
            
            // Stats
            var statsTextComp = item.transform.Find("StatsText")?.GetComponent<TMP_Text>();
            if (statsTextComp != null)
            {
                statsTextComp.text = $"{card.manaCost}⬡ {card.health}♥";
            }
            
            // Add button
            var addButton = item.transform.Find("AddButton")?.GetComponent<Button>();
            if (addButton != null)
            {
                addButton.onClick.AddListener(() => AddCardToDeck(card));
            }
        }
        
        private void RefreshDeckContents()
        {
            ClearContent(deckContentsContent);
            
            if (deckCardItemPrefab == null || deckContentsContent == null) return;
            
            // Group by card for display
            var cardCounts = new Dictionary<CardData, int>();
            foreach (var card in workingDeckCards)
            {
                if (card == null) continue;
                if (!cardCounts.ContainsKey(card))
                    cardCounts[card] = 0;
                cardCounts[card]++;
            }
            
            // Sort by name
            var sortedCards = cardCounts.Keys.OrderBy(c => c.displayName).ToList();
            
            foreach (var card in sortedCards)
            {
                int count = cardCounts[card];
                GameObject item = Instantiate(deckCardItemPrefab, deckContentsContent);
                SetupDeckCardItem(item, card, count);
            }
            
            // Update title
            if (deckTitleText != null && currentDeck != null)
            {
                deckTitleText.text = $"Deck Contents ({workingDeckCards.Count}/{currentDeck.maxCards})";
            }
        }
        
        private void SetupDeckCardItem(GameObject item, CardData card, int count)
        {
            // Name with count
            var nameText = item.transform.Find("NameText")?.GetComponent<TMP_Text>();
            if (nameText != null)
            {
                nameText.text = count > 1 ? $"{card.displayName} x{count}" : card.displayName;
            }
            
            // Stats
            var statsTextComp = item.transform.Find("StatsText")?.GetComponent<TMP_Text>();
            if (statsTextComp != null)
            {
                statsTextComp.text = $"{card.manaCost}⬡ {card.health}♥";
            }
            
            // Remove button
            var removeButton = item.transform.Find("RemoveButton")?.GetComponent<Button>();
            if (removeButton != null)
            {
                removeButton.onClick.AddListener(() => RemoveCardFromDeck(card));
            }
        }
        
        private void AddCardToDeck(CardData card)
        {
            if (currentDeck == null || card == null) return;
            
            // Check max deck size
            if (workingDeckCards.Count >= currentDeck.maxCards)
            {
                Debug.LogWarning($"[DeckEditor] Deck is full ({currentDeck.maxCards} cards max)");
                return;
            }
            
            // Check max copies
            int currentCopies = workingDeckCards.Count(c => c == card);
            if (currentCopies >= currentDeck.maxCopiesPerCard)
            {
                Debug.LogWarning($"[DeckEditor] Already have {currentCopies} copies of {card.displayName} (max {currentDeck.maxCopiesPerCard})");
                return;
            }
            
            workingDeckCards.Add(card);
            hasUnsavedChanges = true;
            
            RefreshDeckContents();
            UpdateStats();
            
            Debug.Log($"[DeckEditor] Added {card.displayName} to deck");
        }
        
        private void RemoveCardFromDeck(CardData card)
        {
            if (currentDeck == null || card == null) return;
            
            // Remove one copy
            int index = workingDeckCards.IndexOf(card);
            if (index >= 0)
            {
                workingDeckCards.RemoveAt(index);
                hasUnsavedChanges = true;
                
                RefreshDeckContents();
                UpdateStats();
                
                Debug.Log($"[DeckEditor] Removed {card.displayName} from deck");
            }
        }
        
        private void UpdateStats()
        {
            if (statsText == null) return;
            
            if (currentDeck == null || workingDeckCards.Count == 0)
            {
                statsText.text = "0 cards │ Avg Mana: - │ Avg HP: -";
                return;
            }
            
            int count = workingDeckCards.Count;
            float avgMana = (float)workingDeckCards.Average(c => c.manaCost);
            float avgHP = (float)workingDeckCards.Average(c => c.health);
            
            string unsaved = hasUnsavedChanges ? " *" : "";
            statsText.text = $"{count} cards │ Avg Mana: {avgMana:F1} │ Avg HP: {avgHP:F1}{unsaved}";
        }
        
        private void OnNewDeckClicked()
        {
#if UNITY_EDITOR
            if (hasUnsavedChanges)
            {
                ShowConfirmDialog("Discard unsaved changes and create new deck?", CreateNewDeck);
            }
            else
            {
                CreateNewDeck();
            }
#endif
        }
        
        private void CreateNewDeck()
        {
#if UNITY_EDITOR
            // Create new deck asset
            DeckData newDeck = ScriptableObject.CreateInstance<DeckData>();
            newDeck.id = System.Guid.NewGuid().ToString();
            newDeck.deckName = "New Deck";
            newDeck.description = "Enter deck description";
            newDeck.cards = new List<CardData>();
            
            // Ensure directory exists
            string dir = "Assets/Resources/Decks";
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            
            // Find unique filename
            string baseName = "new_deck";
            string path = $"{dir}/{baseName}.asset";
            int counter = 1;
            while (System.IO.File.Exists(path))
            {
                path = $"{dir}/{baseName}_{counter}.asset";
                counter++;
            }
            
            AssetDatabase.CreateAsset(newDeck, path);
            AssetDatabase.SaveAssets();
            
            // Refresh and select
            LoadAllDecks();
            RefreshDeckList();
            
            int newIndex = allDecks.IndexOf(newDeck);
            if (newIndex >= 0)
            {
                SelectDeck(newIndex);
            }
            
            Debug.Log($"[DeckEditor] Created new deck: {path}");
#endif
        }
        
        private void OnDeleteDeckClicked()
        {
            if (currentDeck == null) return;
            
            ShowConfirmDialog($"Delete deck '{currentDeck.deckName}'?\nThis cannot be undone.", DeleteCurrentDeck);
        }
        
        private void DeleteCurrentDeck()
        {
#if UNITY_EDITOR
            if (currentDeck == null) return;
            
            string path = AssetDatabase.GetAssetPath(currentDeck);
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[DeckEditor] Deleted deck: {path}");
            }
            
            // Refresh
            LoadAllDecks();
            RefreshDeckList();
            
            if (allDecks.Count > 0)
            {
                SelectDeck(0);
            }
            else
            {
                ClearEditor();
            }
#endif
        }
        
        private void OnSaveClicked()
        {
#if UNITY_EDITOR
            if (currentDeck == null) return;
            
            // Apply working cards to deck
            currentDeck.cards = new List<CardData>(workingDeckCards);
            
            EditorUtility.SetDirty(currentDeck);
            AssetDatabase.SaveAssets();
            
            hasUnsavedChanges = false;
            UpdateStats();
            RefreshDeckList(); // Update card count in list
            
            Debug.Log($"[DeckEditor] Saved deck: {currentDeck.deckName} with {currentDeck.cards.Count} cards");
#endif
        }
        
        #region Confirmation Dialog
        
        private void ShowConfirmDialog(string message, System.Action onConfirm)
        {
            if (confirmDialog == null) return;
            
            pendingConfirmAction = onConfirm;
            
            if (confirmText != null)
            {
                confirmText.text = message;
            }
            
            confirmDialog.SetActive(true);
        }
        
        private void HideConfirmDialog()
        {
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
            pendingConfirmAction = null;
        }
        
        private void OnConfirmYes()
        {
            var action = pendingConfirmAction;
            HideConfirmDialog();
            action?.Invoke();
        }
        
        private void OnConfirmNo()
        {
            HideConfirmDialog();
        }
        
        #endregion
        
        private void ClearContent(Transform content)
        {
            if (content == null) return;
            
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
    }
}
