# Story 012: ScriptableObject Card Data System

## User Story
**As a** game designer  
**I want** cards defined as ScriptableObjects instead of prefabs  
**So that** I can easily create, balance, and manage a large collection of cards

## Background
Currently each card is a separate prefab with stats and abilities attached. This doesn't scale well for 100+ cards. ScriptableObjects allow data-driven card definitions with a single card prefab.

## Acceptance Criteria
- [ ] `CardData` ScriptableObject with all card properties (name, cost, health, attack, abilities, image, rarity, tribe)
- [ ] `AbilityData` ScriptableObject for reusable ability definitions
- [ ] `DeckData` ScriptableObject to define deck compositions
- [ ] Single Card prefab that initializes from `CardData`
- [ ] `CardController` updated to read from `CardData`
- [ ] `CardLibrary` updated to load from `CardData` assets
- [ ] Existing cards migrated to ScriptableObjects
- [ ] Cards can be created via right-click → Create → KOA → Card Data

## Technical Design

### Folder Structure
```
Assets/
├── Data/
│   ├── Cards/
│   │   ├── Creatures/
│   │   └── Spells/
│   ├── Abilities/
│   └── Decks/
├── Prefabs/
│   └── Card.prefab  (single template)
```

### ScriptableObject Classes
```csharp
[CreateAssetMenu(fileName = "NewCard", menuName = "KOA/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public string cardId;
    public Sprite cardImage;
    public int manaCost;
    public int health;
    public int attack;
    public AbilityData offensiveAbility;
    public AbilityData supportAbility;
    public CardRarity rarity;
    public CardTribe tribe;
    public string description;
}

[CreateAssetMenu(fileName = "NewAbility", menuName = "KOA/Ability Data")]
public class AbilityData : ScriptableObject
{
    public string abilityName;
    public AbilityTargetType targetType;
    public AbilityEffectType effectType;
    public int effectValue;
    public Sprite icon;
    public string description;
}

[CreateAssetMenu(fileName = "NewDeck", menuName = "KOA/Deck")]
public class DeckData : ScriptableObject
{
    public string deckName;
    public List<CardData> cards;
}
```

## Tasks
- [ ] Create `CardData.cs` ScriptableObject
- [ ] Create `AbilityData.cs` ScriptableObject
- [ ] Create `DeckData.cs` ScriptableObject
- [ ] Create enum types (CardRarity, CardTribe, AbilityEffectType)
- [ ] Update `CardController` to initialize from `CardData`
- [ ] Update `CardLibrary` to work with ScriptableObjects
- [ ] Create card instantiation factory
- [ ] Migrate existing card prefabs to CardData assets
- [ ] Create sample abilities (DirectDamage, Heal, etc.)
- [ ] Test card creation workflow
- [ ] Test deck building with DeckData

## Dependencies
- Story 011 (Show opponent connection status) - should complete first

## Estimate
3-5 days

## Notes
- Keep backward compatibility during migration
- Consider custom editor for bulk card editing
- Future: JSON import/export for balance spreadsheets

---
*Status: Backlog*
