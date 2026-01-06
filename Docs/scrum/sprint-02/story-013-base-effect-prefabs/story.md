# Story 013: Create Base Effect Prefabs

## Status: Not Started
## Sprint: 02
## Dependencies: 012

---

## User Story

**As a** developer  
**I want** generic effect prefabs that read from AbilityData  
**So that** designers can create most abilities without coding

---

## Description

- AbilityEffect base class with Execute(AbilityData, CardController target)
- GenericDamageEffect - deals data.damage to target
- GenericHealEffect - heals data.healAmount to target
- Effect prefabs in Resources/Effects/ folder
- Effects can be assigned to AbilityData.effectPrefab

---

## Acceptance Criteria

- [ ] Resources/Effects folder contains GenericDamageEffect prefab
- [ ] Resources/Effects folder contains GenericHealEffect prefab
- [ ] AbilityData's effectPrefab field accepts the effect prefabs
- [ ] Playing a card with GenericDamageEffect deals damage to target
- [ ] Playing a card with GenericHealEffect heals the target

---

## Technical Notes

### AbilityEffect.cs (Base Class)
```csharp
public abstract class AbilityEffect : MonoBehaviour
{
    public abstract void Execute(AbilityData data, CardController source, CardController target);
}
```

### GenericDamageEffect.cs
```csharp
public class GenericDamageEffect : AbilityEffect
{
    public override void Execute(AbilityData data, CardController source, CardController target)
    {
        if (target != null)
        {
            target.TakeDamage(data.damage);
        }
    }
}
```

### GenericHealEffect.cs
```csharp
public class GenericHealEffect : AbilityEffect
{
    public override void Execute(AbilityData data, CardController source, CardController target)
    {
        if (target != null)
        {
            target.Heal(data.healAmount);
        }
    }
}
```

### File Locations
- Scripts: `Assets/Scripts/Effects/`
- Prefabs: `Assets/Resources/Effects/`

---

## Tasks

- [ ] Create AbilityEffect base class
- [ ] Create GenericDamageEffect
- [ ] Create GenericHealEffect
- [ ] Create effect prefabs
- [ ] Test execution with dummy AbilityData
