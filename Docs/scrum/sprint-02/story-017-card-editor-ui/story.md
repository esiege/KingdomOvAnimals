# Story 017: Card List & Editor UI

## Status: Not Started
## Sprint: 02
## Dependencies: 012, 016

---

## User Story

**As a** designer  
**I want** to see all cards and edit them  
**So that** I can balance card stats

---

## Acceptance Criteria

- [ ] Left panel: scrollable list of all CardData assets
- [ ] Search/filter by name
- [ ] Right panel: selected card's editable fields
- [ ] Dropdown to assign offensive/defensive abilities
- [ ] Save button persists changes to disk (#if UNITY_EDITOR)
- [ ] New Card button creates new CardData asset
- [ ] Delete button with confirmation

---

## Technical Notes

### UI Layout
```
┌─────────────────────────────────────────────────────────────────┐
│  [← Back]                     Card Editor                       │
├──────────────────────┬──────────────────────────────────────────┤
│  Search: [________]  │  Card: Fire Lion                         │
│                      │                                          │
│  ┌────────────────┐  │  ID:          [fire_lion_______]         │
│  │ ► Fire Lion    │  │  Name:        [Fire Lion_______]         │
│  │   Ice Serpent  │  │  Health:      [3__]                      │
│  │   Stone Golem  │  │                                          │
│  │   Wind Eagle   │  │  Offensive:   [Fireball ▼]               │
│  │                │  │  Defensive:   [Flame Shield ▼]           │
│  │                │  │                                          │
│  │                │  │  Artwork:     [Select...]                │
│  │                │  │                                          │
│  └────────────────┘  │  [Save]  [Delete]                        │
│                      │                                          │
│  [+ New Card]        │                                          │
└──────────────────────┴──────────────────────────────────────────┘
```

### CardEditorUI.cs
```csharp
public class CardEditorUI : MonoBehaviour
{
    [SerializeField] private Transform cardListContent;
    [SerializeField] private GameObject cardListItemPrefab;
    [SerializeField] private TMP_InputField searchField;
    
    // Editor fields
    [SerializeField] private TMP_InputField idField;
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField healthField;
    [SerializeField] private TMP_Dropdown offensiveDropdown;
    [SerializeField] private TMP_Dropdown defensiveDropdown;
    
    private CardData selectedCard;
    private List<CardData> allCards;
    
    public void SaveCard()
    {
        selectedCard.id = idField.text;
        selectedCard.displayName = nameField.text;
        selectedCard.health = int.Parse(healthField.text);
        // ... set abilities from dropdowns
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(selectedCard);
        UnityEditor.AssetDatabase.SaveAssets();
        #endif
    }
    
    public void CreateNewCard()
    {
        #if UNITY_EDITOR
        var newCard = ScriptableObject.CreateInstance<CardData>();
        newCard.id = "new_card_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        newCard.displayName = "New Card";
        newCard.health = 1;
        
        string path = "Assets/Resources/Cards/" + newCard.id + ".asset";
        UnityEditor.AssetDatabase.CreateAsset(newCard, path);
        UnityEditor.AssetDatabase.SaveAssets();
        
        RefreshCardList();
        SelectCard(newCard);
        #endif
    }
}
```

---

## Tasks

- [ ] Create card list UI with scroll view
- [ ] Implement search/filter
- [ ] Create editor panel with input fields
- [ ] Populate ability dropdowns
- [ ] Implement Save functionality
- [ ] Implement New Card functionality
- [ ] Implement Delete with confirmation
