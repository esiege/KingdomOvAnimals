# Card System

## Card Properties

### Core Stats
| Property | Type | Description |
|----------|------|-------------|
| `cardName` | string | Display name of the card |
| `manaCost` | int | Mana required to play the card |
| `health` | int | Current health points |
| `id` | string | Unique GUID for each card instance |

### State Flags
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `isInHand` | bool | true | Card is in player's hand |
| `isActive` | bool | false | Card is currently selected/active |
| `isTapped` | bool | false | Card has been used this turn |
| `isFlipped` | bool | false | Card is face-down |
| `isInPlay` | bool | false | Card is on the board |
| `isHighlighted` | bool | false | Card is highlighted as playable/targetable |

### Status Effects
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `hasSummoningSickness` | bool | true | Cannot attack on the turn it's played |
| `isFrozen` | bool | false | Cannot act (frozen effect) |
| `isBuried` | bool | false | Hidden/buried state |
| `isDefending` | bool | false | In defensive stance |

## Card Abilities

Each card can have two ability slots:
- **Offensive Ability** (`offensiveAbility`) - Used to attack/harm opponents
- **Support Ability** (`supportAbility`) - Used to help friendly targets

## Card Views

Cards have two visual states:
- **Full View** - Displayed when in hand (detailed card info)
- **Condensed View** - Displayed when on the board (compact representation)

Located at:
- `Canvas/Full` - Full card view
- `Canvas/Condensed` - Condensed board view

## Visual Elements

### Status Icons
- `summoningSicknessIcon` - Shown when card has summoning sickness
- `tappedIcon` - Shown when card is tapped
- `flippedIcon` - Shown when card is face-down

### Frames
- `standardFrames` - Default card border appearance
- `highlightedFrames` - Glowing border when card is playable/targetable

## Card Lifecycle

```
1. Card in Deck
       │
       ▼
2. Drawn to Hand
   - isInHand = true
   - Shows "Full" view
       │
       ▼
3. Played to Board
   - isInHand = false
   - isInPlay = true
   - hasSummoningSickness = true
   - Shows "Condensed" view
   - Mana cost deducted
       │
       ▼
4. Ready to Act (Next Turn)
   - hasSummoningSickness = false
   - Can use abilities
       │
       ▼
5. Uses Ability
   - isTapped = true
   - Cannot act again this turn
       │
       ▼
6. Turn Ends
   - isTapped = false (reset)
       │
       ▼
7. Death (health <= 0)
   - Removed from board
   - GameObject destroyed
```

## Card Prefab Structure

```
Card.prefab
├── Canvas/
│   ├── Full/           # Hand view
│   │   ├── CardImage
│   │   ├── NameText
│   │   ├── CostText
│   │   └── HealthText
│   └── Condensed/      # Board view
│       ├── CardImage
│       └── HealthText
├── Abilities/
│   ├── OffensiveAbility
│   └── SupportAbility
└── StatusIcons/
    ├── SummoningSicknessIcon
    ├── TappedIcon
    └── FlippedIcon
```
