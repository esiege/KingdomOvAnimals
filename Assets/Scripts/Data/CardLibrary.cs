using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that provides access to card prefabs by name.
/// Used for state restoration after reconnection.
/// </summary>
public class CardLibrary : MonoBehaviour
{
    public static CardLibrary Instance { get; private set; }
    
    [Header("Card Prefabs")]
    [Tooltip("All card prefabs that can be spawned. Populate this in the inspector or call AutoPopulateFromScene.")]
    public List<CardController> cardPrefabs = new List<CardController>();
    
    // Lookup dictionary built at runtime - stores original card templates
    private Dictionary<string, CardController> _cardLookup = new Dictionary<string, CardController>();
    
    // Flag to track if we've auto-populated
    private bool _hasPopulated = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("[CardLibrary] Duplicate instance found, destroying this one");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        BuildLookup();
    }
    
    private void BuildLookup()
    {
        _cardLookup.Clear();
        foreach (var card in cardPrefabs)
        {
            if (card != null && !string.IsNullOrEmpty(card.cardName))
            {
                if (!_cardLookup.ContainsKey(card.cardName))
                {
                    _cardLookup[card.cardName] = card;
                    Debug.Log($"[CardLibrary] Registered card: {card.cardName}");
                }
                else
                {
                    Debug.LogWarning($"[CardLibrary] Duplicate card name: {card.cardName}");
                }
            }
        }
        if (_cardLookup.Count > 0)
        {
            Debug.Log($"[CardLibrary] Built lookup with {_cardLookup.Count} cards");
        }
    }
    
    /// <summary>
    /// Get a card prefab/template by name.
    /// </summary>
    public CardController GetCardTemplate(string cardName)
    {
        if (_cardLookup.TryGetValue(cardName, out CardController card))
        {
            return card;
        }
        Debug.LogWarning($"[CardLibrary] Card not found: {cardName}");
        return null;
    }
    
    /// <summary>
    /// Instantiate a card by name.
    /// </summary>
    public CardController InstantiateCard(string cardName, Transform parent = null)
    {
        CardController template = GetCardTemplate(cardName);
        if (template == null) return null;
        
        GameObject cardObj = Instantiate(template.gameObject, parent);
        return cardObj.GetComponent<CardController>();
    }
    
    /// <summary>
    /// Check if a card exists in the library.
    /// </summary>
    public bool HasCard(string cardName)
    {
        return _cardLookup.ContainsKey(cardName);
    }
    
    /// <summary>
    /// Get all registered card names.
    /// </summary>
    public IEnumerable<string> GetAllCardNames()
    {
        return _cardLookup.Keys;
    }
    
    /// <summary>
    /// Get all registered cards.
    /// </summary>
    public Dictionary<string, CardController>.ValueCollection GetAllCards()
    {
        return _cardLookup.Values;
    }
    
    /// <summary>
    /// Auto-populate the library from existing decks in the scene.
    /// Should be called early in game startup (e.g., from EncounterController).
    /// </summary>
    public void AutoPopulateFromScene()
    {
        if (_hasPopulated)
        {
            Debug.Log("[CardLibrary] Already populated, skipping");
            return;
        }
        
        // Find all PlayerControllers and gather their deck cards
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        Debug.Log($"[CardLibrary] Found {players.Length} PlayerController(s)");
        
        foreach (var player in players)
        {
            Debug.Log($"[CardLibrary] Checking {player.name}: deck={(player.deck != null ? player.deck.Count.ToString() : "null")}, board={(player.board != null ? player.board.Count.ToString() : "null")}");
            
            if (player.deck != null)
            {
                foreach (var card in player.deck)
                {
                    if (card != null && !string.IsNullOrEmpty(card.cardName))
                    {
                        if (!_cardLookup.ContainsKey(card.cardName))
                        {
                            // Store a reference to the original card as a "template"
                            _cardLookup[card.cardName] = card;
                            Debug.Log($"[CardLibrary] Auto-registered card from scene: {card.cardName}");
                        }
                    }
                }
            }
            
            // Also check board for cards that might already be in play
            if (player.board != null)
            {
                foreach (var card in player.board)
                {
                    if (card != null && !string.IsNullOrEmpty(card.cardName))
                    {
                        if (!_cardLookup.ContainsKey(card.cardName))
                        {
                            _cardLookup[card.cardName] = card;
                            Debug.Log($"[CardLibrary] Auto-registered board card from scene: {card.cardName}");
                        }
                    }
                }
            }
        }
        
        // Fallback: If still no cards found, search ALL CardControllers in the scene
        if (_cardLookup.Count == 0)
        {
            Debug.Log("[CardLibrary] No cards found in PlayerController decks, searching all CardControllers...");
            CardController[] allCards = FindObjectsOfType<CardController>(true); // Include inactive
            Debug.Log($"[CardLibrary] Found {allCards.Length} CardController(s) in scene");
            
            foreach (var card in allCards)
            {
                if (card != null && !string.IsNullOrEmpty(card.cardName))
                {
                    if (!_cardLookup.ContainsKey(card.cardName))
                    {
                        _cardLookup[card.cardName] = card;
                        Debug.Log($"[CardLibrary] Fallback registered: {card.cardName}");
                    }
                }
            }
        }
        
        _hasPopulated = true;
        Debug.Log($"[CardLibrary] Auto-populate complete. Total cards: {_cardLookup.Count}");
    }
    
    /// <summary>
    /// Force re-population (useful for reconnection scenarios).
    /// </summary>
    public void ForceRepopulate()
    {
        _hasPopulated = false;
        _cardLookup.Clear();
        AutoPopulateFromScene();
    }
    
    /// <summary>
    /// Ensure the library is ready. Call this before using.
    /// </summary>
    public static void EnsureInitialized()
    {
        if (Instance == null)
        {
            // Create the CardLibrary if it doesn't exist
            var go = new GameObject("CardLibrary");
            Instance = go.AddComponent<CardLibrary>();
        }
        
        if (!Instance._hasPopulated)
        {
            Instance.AutoPopulateFromScene();
        }
    }
}
