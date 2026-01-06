# Story 018: Ability List & Editor UI

## Status: Not Started
## Sprint: 02
## Dependencies: 012, 016

---

## User Story

**As a** designer  
**I want** to see all abilities and edit their values  
**So that** I can design and balance abilities

---

## Description

- List of all AbilityData assets
- Edit: name, description, mana cost
- Edit: damage, healAmount, duration values
- Dropdown for targetType, animationType
- Dropdown for effectPrefab (shows available effect prefabs)
- Save persists to disk
- New/Delete ability buttons

---

## Acceptance Criteria

- [ ] I see a list of all abilities on the left side
- [ ] Clicking an ability shows its details on the right
- [ ] I can edit name, description, mana cost, damage, heal, duration
- [ ] I can select target type from a dropdown
- [ ] I can select effect prefab from a dropdown
- [ ] Clicking Save updates the ability (persists after exiting play mode)
- [ ] Clicking New Ability adds a new ability to the list
- [ ] Clicking Delete removes the ability after confirmation

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
