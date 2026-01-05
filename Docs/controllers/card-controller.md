# CardController

Represents a card instance in play or hand. Manages card state and visuals.

## Responsibilities

- Display card data (art, stats, text)
- Track in-play state flags
- Handle visual state changes
- Execute ability triggers

## Data Binding

| Property | Source |
|----------|--------|
| `cardTemplate` | CardTemplate ScriptableObject |
| `cardNameText` | template.cardName |
| `manaCostText` | template.manaCost |
| `attackText` | template.attack |
| `healthText` | template.health |
| `abilityText` | template.abilityDescription |

## State Flags

| Flag | Description |
|------|-------------|
| `isInPlay` | Card is on board (not in hand/deck/graveyard) |
| `isTapped` | Has attacked this turn |
| `hasSummoningSickness` | Cannot attack this turn (just played) |
| `isTargetable` | Can be targeted by abilities |

## Stats

| Property | Description |
|----------|-------------|
| `currentAttack` | Modified attack value |
| `currentHealth` | Current health (dies at 0) |
| `maxHealth` | Maximum health for healing cap |

## Owner Reference

| Property | Description |
|----------|-------------|
| `ownerPlayer` | PlayerController who owns this card |
| `isOwnedByLocalPlayer` | True if local player's card |

## Visual States

| State | Visual Change |
|-------|--------------|
| Tapped | Rotated 90 degrees |
| Summoning Sick | Dimmed/grayed |
| Targetable | Highlight glow |
| Damaged | Red flash |
| Buffed | Green number text |
| Debuffed | Red number text |

## Methods

| Method | Description |
|--------|-------------|
| `Initialize(template)` | Set up card from template |
| `TakeDamage(amount)` | Reduce health, check death |
| `Heal(amount)` | Restore health up to max |
| `Tap()` | Mark as tapped |
| `Untap()` | Clear tapped flag |
| `ClearSummoningSickness()` | Allow attacks |
| `Die()` | Move to graveyard |

## Events

| Event | Trigger |
|-------|---------|
| `OnDamaged` | Card takes damage |
| `OnHealed` | Card healed |
| `OnDeath` | Health reaches 0 |
| `OnStatsChanged` | Attack or health modified |

## Network Sync

Card state synchronized via:
- `NetworkPlayer.SyncBoardState()` for full reconciliation
- Individual ability/attack RPCs for incremental updates

---
*Parent: [Controllers](./README.md)*
