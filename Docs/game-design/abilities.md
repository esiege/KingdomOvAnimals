# Ability System

## Overview

Abilities are the core mechanic of Kingdom Ov Animals. Unlike traditional card games with attack/health stats, **cards have no inherent attack value** - instead, they have:

- **Health** - How much damage they can take
- **Offensive Ability** - How they damage/affect enemies
- **Defensive Ability** - How they help allies

This makes every card unique and allows for rich, diverse effects beyond simple combat.

## Card-Ability Relationship

```
Card = Container
├── Name, ManaCost, Health
├── Offensive Ability → "How I hurt enemies"
└── Defensive Ability → "How I help allies"

Traditional "3/5" creature equivalent:
├── Health: 5
└── Offensive Ability: Deal 3 Damage
```

---

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
| `RandomEnemy` | Random enemy card | Chaotic effects |
| `RandomFriendly` | Random friendly card | Risky heals |

---

## Effect Types

### Damage Effects
| Effect | Description | Parameters |
|--------|-------------|------------|
| **Damage** | Deal instant damage | value |
| **Poison** | Deal damage over turns | value, duration |
| **Burn** | Deal damage at turn start | value, duration |
| **Lifesteal** | Damage + heal self | value |

### Healing Effects
| Effect | Description | Parameters |
|--------|-------------|------------|
| **Heal** | Restore health instantly | value |
| **Regeneration** | Heal over turns | value, duration |
| **Shield** | Absorb next X damage | value |

### Control Effects
| Effect | Description | Parameters |
|--------|-------------|------------|
| **Stun** | Target skips next action | duration |
| **Freeze** | Target can't act | duration |
| **Silence** | Target can't use abilities | duration |
| **Taunt** | Must be attacked first | - |
| **Stealth** | Can't be targeted | duration |

### Movement Effects
| Effect | Description | Parameters |
|--------|-------------|------------|
| **ReturnToHand** | Bounce card to hand | - |
| **MoveToSlot** | Reposition on board | slotIndex |
| **Swap** | Swap with another card | - |
| **Steal** | Take control of enemy | duration |

### Card Advantage
| Effect | Description | Parameters |
|--------|-------------|------------|
| **Draw** | Draw cards | value |
| **Discard** | Force discard | value |
| **Mill** | Cards from deck to graveyard | value |
| **Tutor** | Search deck for card | cardType |

### Buff/Debuff Effects
| Effect | Description | Parameters |
|--------|-------------|------------|
| **BuffDamage** | +X to ability damage | value, duration |
| **DebuffDamage** | -X to ability damage | value, duration |
| **BuffHealth** | +X max health | value |
| **ReduceHealth** | -X max health | value |
| **BuffManaCost** | Reduce mana cost | value |

### Special Effects
| Effect | Description | Parameters |
|--------|-------------|------------|
| **Summon** | Create a creature | cardId |
| **Copy** | Clone a creature | - |
| **Transform** | Become different card | cardId |
| **Destroy** | Instantly kill | - |
| **Resurrect** | Return from graveyard | - |

---

## Composable Effects

Abilities can stack multiple effects for complex behaviors:

### Example: Venomous Strike
```yaml
name: "Venomous Strike"
targetType: SingleEnemy
effects:
  - type: Damage, value: 2
  - type: Poison, value: 1, duration: 3
animation: PoisonSlash
description: "Deal 2 damage. Poison for 1 damage over 3 turns."
```

### Example: Life Drain
```yaml
name: "Life Drain"
targetType: SingleEnemy
effects:
  - type: Damage, value: 3
  - type: Heal, value: 2, target: Self
animation: DarkDrain
description: "Deal 3 damage. Heal self for 2."
```

### Example: Frostbite
```yaml
name: "Frostbite"
targetType: SingleEnemy
effects:
  - type: Damage, value: 1
  - type: Freeze, duration: 1
animation: IceBlast
description: "Deal 1 damage and freeze for 1 turn."
```

---

## Animation Types

| Animation | Used For | VFX |
|-----------|----------|-----|
| Slash | Physical damage | Claw/sword marks |
| Fireball | Fire damage | Flames, explosion |
| IceBlast | Freeze effects | Ice crystals |
| PoisonCloud | Poison effects | Green mist |
| HealingLight | Heals | Green/gold particles |
| DarkDrain | Lifesteal | Purple tendrils |
| Lightning | Electric damage | Sparks, chains |
| Earthquake | AoE damage | Screen shake |
| Summon | Creature creation | Portal, glow |
| Buff | Stat increases | Upward arrows |
| Debuff | Stat decreases | Downward arrows |

---

## Status Effect Duration

Status effects tick at specific times:

| Timing | Effects |
|--------|---------|
| **Start of Turn** | Poison damage, Regeneration heal, Freeze check |
| **End of Turn** | Duration countdown |
| **On Attack** | Lifesteal triggers |
| **On Damage Taken** | Shield absorption |

---

## Ability Execution Flow

```
1. Player drags card to target (from hand or board)
       │
       ▼
2. Validate action
   - Has enough mana (if from hand)
   - Can act (not tapped, no summoning sickness)
       │
       ▼
3. Get valid targets based on AbilityTargetType
       │
       ▼
4. Execute each Effect in ability's effect list
   - Apply damage/healing
   - Apply status effects
   - Trigger animations
       │
       ▼
5. Post-execution
   - Card is tapped (if from board)
   - Card is consumed (if from hand, for spells)
   - Update UI
       │
       ▼
6. Check for triggered effects
   - On-damage triggers
   - Death triggers
   - etc.
```

---

## Future Considerations

### Keywords
Common effect combinations as keywords:
- **Venomous** = Poison on hit
- **Vampiric** = Lifesteal
- **Frosty** = Freeze on hit
- **Explosive** = Damage adjacent enemies

### Triggered Abilities
- **On Play** - When card enters board
- **On Death** - When card dies
- **On Damage** - When card takes damage
- **On Turn Start** - Each turn
- **On Turn End** - Each turn

### Conditional Effects
- "If enemy is poisoned, deal double damage"
- "If you have 3+ cards in hand, draw 1"
- "If health below 5, gain +2 damage"

---

*Parent: [Game Design](./README.md)*
