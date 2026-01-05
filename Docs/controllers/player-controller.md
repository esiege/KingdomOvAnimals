# PlayerController

Manages a single player's state and resources. One instance per player per match.

## Responsibilities

- Track health and mana
- Manage deck and graveyard
- Coordinate with board slots
- Handle card play validation
- Sync with NetworkPlayer in multiplayer

## Identity

| Property | Description |
|----------|-------------|
| `playerType` | Enum: `Player` or `Opponent` |
| `playerName` | Display name |
| `isLocalPlayer` | True if controlled by this client |

## Resources

| Property | Type | Description |
|----------|------|-------------|
| `health` | int | Current health points |
| `maxHealth` | int | Maximum health (default 20) |
| `mana` | int | Available mana this turn |
| `maxMana` | int | Current mana cap (increases per turn) |

## Collections

| Property | Description |
|----------|-------------|
| `deck` | List of CardTemplate remaining in deck |
| `graveyard` | Cards removed from play |
| `boardSlots` | Transform[] for board positions |

## Network Integration

When `NetworkPlayer` is set:
- Health/mana changes sync automatically via SyncVars
- Card plays route through ServerRpcs
- Board state reconciled on reconnect

### Key Networked Methods

| Method | Network Behavior |
|--------|-----------------|
| `TakeDamage()` | Updates health, synced via NetworkPlayer |
| `SpendMana()` | Validates and deducts mana |
| `AddMana()` | Called on turn start |
| `DrawCard()` | Moves from deck to hand |

## Card Play Flow

1. `CanPlayCard(card)` - Check mana cost, board space
2. `PlayCard(card, slot)` - Remove from hand, place on board
3. Network: `NetworkPlayer.CmdPlayCard()` called
4. Server validates and broadcasts to all clients

## Board Management

| Method | Description |
|--------|-------------|
| `GetEmptySlot()` | Find first available board slot |
| `GetBoardCards()` | All CardController on board |
| `ResetBoard()` | Clear tapped, summoning sickness |

## Events

| Event | Trigger |
|-------|---------|
| `OnHealthChanged` | Health value changes |
| `OnManaChanged` | Mana value changes |
| `OnCardPlayed` | Card moved to board |
| `OnCardDied` | Card destroyed |

---
*Parent: [Controllers](./README.md)*
