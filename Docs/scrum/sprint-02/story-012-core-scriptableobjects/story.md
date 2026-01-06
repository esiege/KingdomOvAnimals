# Story 012: Create Core Data ScriptableObjects

## Status: Not Started
## Sprint: 02
## Dependencies: None

---

## User Story

**As a** developer  
**I want** CardData, AbilityData ScriptableObject classes  
**So that** card definitions are data-driven and editable

---

## Description

- CardData SO with: id, displayName, health, offensiveAbility, defensiveAbility, artwork
- AbilityData SO with: id, displayName, description, manaCost, damage, healAmount, duration, targetType, effectPrefab, animationType, vfxPrefab
- TargetType enum (Self, SingleEnemy, AllEnemies, SingleAlly, AllAllies)
- AnimationType enum (Melee, Projectile, AOE, Buff, etc.)
- [CreateAssetMenu] attributes for manual creation if needed
- Proper serialization (shows in inspector)

---

## Acceptance Criteria

- [ ] Right-click in Project > Create > KOA shows "Card Data" option
- [ ] Right-click in Project > Create > KOA shows "Ability Data" option
- [ ] Creating a CardData asset shows all fields in the Inspector
- [ ] Creating an AbilityData asset shows all fields in the Inspector
- [ ] Target Type dropdown shows: Self, SingleEnemy, AllEnemies, SingleAlly, AllAllies
- [ ] Animation Type dropdown shows: Melee, Projectile, AOE, Buff, etc.

---

## Technical Notes

### CardData.cs
```csharp
[CreateAssetMenu(fileName = "NewCard", menuName = "KOA/Card Data")]
public class CardData : ScriptableObject
{
    public string id;
    public string displayName;
    public int health;
    public Sprite artwork;
    public AbilityData offensiveAbility;
    public AbilityData defensiveAbility;
}
```

### AbilityData.cs
```csharp
[CreateAssetMenu(fileName = "NewAbility", menuName = "KOA/Ability Data")]
public class AbilityData : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public int manaCost;
    
    // Effect values
    public int damage;
    public int healAmount;
    public int duration;
    
    // Targeting
    public TargetType targetType;
    
    // Behavior & Visuals
    public GameObject effectPrefab;
    public AnimationType animationType;
    public GameObject vfxPrefab;
}
```

### File Location
`Assets/Scripts/Data/`

---

## Tasks

- [ ] Create TargetType enum
- [ ] Create AnimationType enum
- [ ] Create AbilityData ScriptableObject
- [ ] Create CardData ScriptableObject
- [ ] Test creating assets via Unity menu
- [ ] Verify serialization in inspector
