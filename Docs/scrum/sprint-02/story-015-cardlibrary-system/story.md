# Story 015: Create CardLibrary System

## Status: Complete ✓
## Sprint: 02
## Dependencies: 012, 014
## Started: 2026-01-06
## Completed: 2026-01-06

---

## User Story

**As a** developer  
**I want** a CardLibrary that loads all CardData assets  
**So that** cards can be looked up by ID at runtime

---

## Description

- CardLibrary singleton loads all CardData from Resources
- GetCardById(string id) returns CardData
- GetAllCards() returns list for deck building
- GetAbilityById(string id) returns AbilityData
- Works in builds (not editor-only)

---

## Acceptance Criteria

- [ ] CardLibrary GameObject exists in NetworkManager scene
- [ ] Playing the game, all cards load correctly from Resources
- [ ] Console shows no "card not found" or loading errors
- [ ] Building the game succeeds without errors
- [ ] Running the build, cards still load and work correctly

---

## Technical Notes

### CardLibrary.cs
```csharp
public class CardLibrary : MonoBehaviour
{
    public static CardLibrary Instance { get; private set; }
    
    private Dictionary<string, CardData> cards = new();
    private Dictionary<string, AbilityData> abilities = new();
    
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAllData();
    }
    
    private void LoadAllData()
    {
        var allCards = Resources.LoadAll<CardData>("Cards");
        foreach (var card in allCards)
        {
            cards[card.id] = card;
        }
        
        var allAbilities = Resources.LoadAll<AbilityData>("Abilities");
        foreach (var ability in allAbilities)
        {
            abilities[ability.id] = ability;
        }
    }
    
    public CardData GetCardById(string id) => cards.GetValueOrDefault(id);
    public AbilityData GetAbilityById(string id) => abilities.GetValueOrDefault(id);
    public List<CardData> GetAllCards() => cards.Values.ToList();
    public List<AbilityData> GetAllAbilities() => abilities.Values.ToList();
}
```

### Data Locations
- Cards: `Assets/Resources/Cards/`
- Abilities: `Assets/Resources/Abilities/`

---

## Tasks

- [ ] Create CardLibrary singleton
- [ ] Implement LoadAllData from Resources
- [ ] Add GetCardById, GetAbilityById methods
- [ ] Add GetAllCards, GetAllAbilities methods
- [ ] Test loading in builds
