# Ability System

## Overview

Abilities are the primary way cards interact with the game. Each ability is a MonoBehaviour that inherits from `AbilityController` and implements targeting and activation logic.

## AbilityController (Base Class)

```csharp
public abstract class AbilityController : MonoBehaviour
{
    public AbilityTargetType targetType;
    
    public abstract void Activate(CardController target);
    public abstract void Activate(PlayerController target);
}
```

All abilities must implement two `Activate` overloads:
1. For targeting cards
2. For targeting players

## Target Types

| Target Type | Description | Example Use |
|-------------|-------------|-------------|
| `SingleEnemy` | One enemy card | Direct damage, debuffs |
| `SingleFriendly` | One friendly card | Heals, buffs |
| `AllEnemies` | All enemy cards | AoE damage |
| `AllFriendlies` | All friendly cards | Mass heal |
| `Self` | The card itself | Self-buffs |
| `PlayerOnly` | Opponent player | Direct player damage |
| `BoardWide` | All cards on board | Board clears |

## Implemented Abilities

### DamageAbility
Deals damage to a target card or player.

**Properties:**
- `damageAmount` (int) - Amount of damage to deal

**Behavior:**
- On card target: Calls `target.TakeDamage(damageAmount)`
- On player target: Calls `target.TakeDamage(damageAmount)`

### Ability_Heal
Restores health to a target card or player.

**Properties:**
- `healAmount` (int) - Amount of health to restore

**Behavior:**
- On card target: Calls `target.Heal(healAmount)`
- On player target: Calls `target.Heal(healAmount)`

## Creating New Abilities

1. Create a new script inheriting from `AbilityController`
2. Set the `targetType` in the Inspector
3. Implement both `Activate` methods
4. Add UI elements for displaying ability stats (optional)

### Template

```csharp
using UnityEngine;
using TMPro;

public class NewAbility : AbilityController
{
    public int effectValue = 5;
    public TextMeshProUGUI effectText;

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (effectText != null)
            effectText.text = effectValue.ToString();
    }

    public override void Activate(CardController target)
    {
        if (target != null)
        {
            // Apply effect to card
            Debug.Log($"Applying effect to {target.cardName}");
        }
    }

    public override void Activate(PlayerController target)
    {
        if (target != null)
        {
            // Apply effect to player
            Debug.Log($"Applying effect to player {target.name}");
        }
    }
}
```

## Ability Execution Flow

```
1. Player selects a card on board
       │
       ▼
2. HandController checks if card can act
   - Not tapped
   - No summoning sickness
       │
       ▼
3. TargetingController.GetOffensiveTargets() or GetSupportTargets()
   - Returns valid targets based on AbilityTargetType
       │
       ▼
4. Valid targets are highlighted
       │
       ▼
5. Player selects a target
       │
       ▼
6. Ability.Activate(target) is called
       │
       ▼
7. Card is tapped (isTapped = true)
```

## Ability Prefabs Location

```
Assets/Prefabs/Card/Abilities/
├── DamageAbility.prefab
├── HealAbility.prefab
└── [Future abilities...]
```
