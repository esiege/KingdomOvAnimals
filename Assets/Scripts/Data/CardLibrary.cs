using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KOA.Data;

/// <summary>
/// Singleton that provides access to card data by ID.
/// Used for looking up CardData assets from Resources/Cards.
/// 
/// Story 036: All old CardController prefab methods have been removed.
/// Use GetCardDataById() / GetCardById() for the new architecture.
/// </summary>
public class CardLibrary : MonoBehaviour
{
    public static CardLibrary Instance { get; private set; }
    
    // CardData and AbilityData lookups (loaded from Resources)
    private Dictionary<string, CardData> _cardDataLookup = new Dictionary<string, CardData>();
    private Dictionary<string, AbilityData> _abilityDataLookup = new Dictionary<string, AbilityData>();
    private List<CardData> _allCardDataList = new List<CardData>();
    private List<AbilityData> _allAbilityDataList = new List<AbilityData>();
    
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
        
        LoadCardDataAssets();
        LoadAbilityDataAssets();
    }
    
    /// <summary>
    /// Load all CardData assets from Resources/Cards folder.
    /// </summary>
    private void LoadCardDataAssets()
    {
        _cardDataLookup.Clear();
        _allCardDataList.Clear();
        
        var loadedCards = Resources.LoadAll<CardData>("Cards");
        
        foreach (var card in loadedCards)
        {
            string id = !string.IsNullOrEmpty(card.id) ? card.id : card.name;
            
            if (_cardDataLookup.ContainsKey(id))
            {
                Debug.LogWarning($"[CardLibrary] Duplicate CardData ID: {id}");
                continue;
            }
            
            _cardDataLookup[id] = card;
            _allCardDataList.Add(card);
        }
        
        if (_cardDataLookup.Count > 0)
        {
            Debug.Log($"[CardLibrary] Loaded {_cardDataLookup.Count} CardData assets from Resources/Cards");
        }
    }
    
    /// <summary>
    /// Load all AbilityData assets from Resources/Abilities folder.
    /// </summary>
    private void LoadAbilityDataAssets()
    {
        _abilityDataLookup.Clear();
        _allAbilityDataList.Clear();
        
        var loadedAbilities = Resources.LoadAll<AbilityData>("Abilities");
        
        foreach (var ability in loadedAbilities)
        {
            string id = !string.IsNullOrEmpty(ability.id) ? ability.id : ability.name;
            
            if (_abilityDataLookup.ContainsKey(id))
            {
                Debug.LogWarning($"[CardLibrary] Duplicate AbilityData ID: {id}");
                continue;
            }
            
            _abilityDataLookup[id] = ability;
            _allAbilityDataList.Add(ability);
        }
        
        if (_abilityDataLookup.Count > 0)
        {
            Debug.Log($"[CardLibrary] Loaded {_abilityDataLookup.Count} AbilityData assets from Resources/Abilities");
        }
    }
    
    #region CardData API (New System)
    
    /// <summary>
    /// Get a CardData asset by its unique ID.
    /// </summary>
    public CardData GetCardDataById(string id)
    {
        if (_cardDataLookup.TryGetValue(id, out var cardData))
        {
            return cardData;
        }
        return null;
    }
    
    /// <summary>
    /// Alias for GetCardDataById - used by new Model/View system.
    /// </summary>
    public CardData GetCardById(string id) => GetCardDataById(id);
    
    /// <summary>
    /// Get an AbilityData asset by its unique ID.
    /// </summary>
    public AbilityData GetAbilityDataById(string id)
    {
        if (_abilityDataLookup.TryGetValue(id, out var abilityData))
        {
            return abilityData;
        }
        return null;
    }
    
    /// <summary>
    /// Get all loaded CardData assets.
    /// </summary>
    public List<CardData> GetAllCardData()
    {
        return new List<CardData>(_allCardDataList);
    }
    
    /// <summary>
    /// Get all loaded AbilityData assets.
    /// </summary>
    public List<AbilityData> GetAllAbilityData()
    {
        return new List<AbilityData>(_allAbilityDataList);
    }
    
    /// <summary>
    /// Get CardData assets filtered by type.
    /// </summary>
    public List<CardData> GetCardDataByType(CardType type)
    {
        return _allCardDataList.Where(c => c.cardType == type).ToList();
    }
    
    /// <summary>
    /// Check if a CardData exists.
    /// </summary>
    public bool HasCardData(string id)
    {
        return _cardDataLookup.ContainsKey(id);
    }
    
    /// <summary>
    /// Check if an AbilityData exists.
    /// </summary>
    public bool HasAbilityData(string id)
    {
        return _abilityDataLookup.ContainsKey(id);
    }
    
    /// <summary>
    /// Get count of loaded CardData assets.
    /// </summary>
    public int CardDataCount => _cardDataLookup.Count;
    
    /// <summary>
    /// Get count of loaded AbilityData assets.
    /// </summary>
    public int AbilityDataCount => _abilityDataLookup.Count;
    
    /// <summary>
    /// Reload CardData and AbilityData from Resources (useful for editor).
    /// </summary>
    public void ReloadDataAssets()
    {
        LoadCardDataAssets();
        LoadAbilityDataAssets();
    }
    
    #endregion
    
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
    }
}
