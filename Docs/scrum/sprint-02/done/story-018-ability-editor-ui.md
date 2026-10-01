# Story 018: Ability List & Editor UI

## Status: Complete ✓
## Sprint: 02
## Dependencies: 012, 016
## Started: 2026-01-06
## Completed: 2026-01-06

---

## User Story

**As a** designer  
**I want** to see all abilities and edit their values  
**So that** I can design and balance abilities

---

## Description

- List of all AbilityData assets
- Edit: name, description
- Behavior dropdown - selects the AbilityBehavior class that executes the ability
- Conditional fields (damage, healAmount, duration) shown based on behavior's RequiredFields
- Dropdown for targetType, animationType
- Dropdown for effectPrefab (shows available effect prefabs)
- Save persists to disk
- New/Delete ability buttons

---

## Acceptance Criteria

- [x] I see a list of all abilities on the left side
- [x] Clicking an ability shows its details on the right
- [x] I can edit name, description, and conditional fields based on behavior
- [x] I can select behavior from a dropdown (Damage, Heal, Poison, Stun, etc.)
- [x] I can select target type from a dropdown
- [x] I can select effect prefab from a dropdown
- [x] Clicking Save updates the ability (persists after exiting play mode)
- [x] Clicking New Ability adds a new ability to the list
- [x] Clicking Delete removes the ability after confirmation

---

## Technical Notes

### UI Layout
```
┌─────────────────────────────────────────────────────────────────┐
│  [← Back]                   Ability Editor                      │
├──────────────────────┬──────────────────────────────────────────┤
│  Search: [________]  │  Ability: Fireball                       │
│                      │                                          │
│  ┌────────────────┐  │  ID:          [fireball________]         │
│  │ ► Fireball     │  │  Name:        [Fireball________]         │
│  │   Ice Shard    │  │  Description: [Deal {damage} fire ]      │
│  │   Heal         │  │               [damage to target__]       │
│  │   Shield       │  │  Mana Cost:   [3__]                      │
│  │                │  │                                          │
│  │                │  │  ─── Effect Values ───                   │
│  │                │  │  Damage:      [5__]                      │
│  │                │  │  Heal Amount: [0__]                      │
│  │                │  │  Duration:    [0__]                      │
│  │                │  │                                          │
│  └────────────────┘  │  ─── Targeting & Behavior ───            │
│                      │  Target Type: [SingleEnemy ▼]            │
│  [+ New Ability]     │  Effect:      [GenericDamageEffect ▼]    │
│                      │  Animation:   [Projectile ▼]             │
│                      │                                          │
│                      │  [Save]  [Delete]                        │
└──────────────────────┴──────────────────────────────────────────┘
```

### AbilityEditorUI.cs
```csharp
public class AbilityEditorUI : MonoBehaviour
{
    [SerializeField] private Transform abilityListContent;
    [SerializeField] private GameObject abilityListItemPrefab;
    
    // Editor fields
    [SerializeField] private TMP_InputField idField;
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField descriptionField;
    [SerializeField] private TMP_InputField manaCostField;
    [SerializeField] private TMP_InputField damageField;
    [SerializeField] private TMP_InputField healAmountField;
    [SerializeField] private TMP_InputField durationField;
    [SerializeField] private TMP_Dropdown targetTypeDropdown;
    [SerializeField] private TMP_Dropdown effectPrefabDropdown;
    [SerializeField] private TMP_Dropdown animationTypeDropdown;
    
    private AbilityData selectedAbility;
    
    private void Start()
    {
        // Populate dropdowns from enums
        PopulateEnumDropdown<TargetType>(targetTypeDropdown);
        PopulateEnumDropdown<AnimationType>(animationTypeDropdown);
        PopulateEffectPrefabDropdown();
    }
    
    private void PopulateEffectPrefabDropdown()
    {
        // Load all effect prefabs from Resources/Effects
        var effects = Resources.LoadAll<AbilityEffect>("Effects");
        effectPrefabDropdown.ClearOptions();
        effectPrefabDropdown.AddOptions(effects.Select(e => e.name).ToList());
    }
}
```

---

## Tasks

- [ ] Create ability list UI with scroll view
- [ ] Create editor panel with all fields
- [ ] Populate enum dropdowns (TargetType, AnimationType)
- [ ] Populate effect prefab dropdown from Resources
- [ ] Implement Save functionality
- [ ] Implement New/Delete abilities
