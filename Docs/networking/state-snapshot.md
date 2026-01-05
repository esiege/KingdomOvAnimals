# GameStateSnapshot

Serializable representation of complete game state for persistence and reconnection.

## Purpose

- Capture game state when disconnection occurs
- Serialize to JSON for network transfer
- Restore state after reconnection
- Enable game state persistence (future feature)

## Structure

```
GameStateSnapshot
├── turnNumber: int
├── currentTurnObjectId: int
├── shuffleSeed: int
├── timestamp: long
├── isFromServerPerspective: bool
├── localPlayer: PlayerSnapshot
│   ├── playerId: int
│   ├── health: int
│   ├── mana: int
│   ├── maxMana: int
│   ├── handCardIds: List<string>
│   ├── boardCards: List<BoardCardSnapshot>
│   ├── deckCardIds: List<string>
│   └── graveyardCardIds: List<string>
└── opponent: PlayerSnapshot
    └── (same structure)
```

## BoardCardSnapshot

Captures individual card state on the board:

| Field | Type | Description |
|-------|------|-------------|
| `slotIndex` | int | Board slot (0-2) |
| `slotName` | string | Slot GameObject name |
| `cardId` | string | Card identifier/name |
| `currentHealth` | int | Current HP |
| `maxHealth` | int | Max HP |
| `currentAttack` | int | Attack value |
| `hasAttacked` | bool | Used this turn |
| `hasSummoningSickness` | bool | Just played |
| `canAttack` | bool | Can act this turn |

## Capturing State

`CaptureState(EncounterController, NetworkGameManager)`:

1. Records turn information from NetworkGameManager
2. Captures PlayerSnapshot for local player
3. Captures PlayerSnapshot for opponent
4. Records timestamp for validation

PlayerSnapshot capture:
- Prefers NetworkPlayer SyncVar values if available
- Falls back to PlayerController local values
- Collects hand cards from HandController
- Collects board cards with positions

## Serialization

Uses Unity's `JsonUtility`:

```
snapshot.ToJson() → JSON string
GameStateSnapshot.FromJson(json) → GameStateSnapshot
```

## Perspective Flag

`isFromServerPerspective` indicates who captured the snapshot:
- `true`: Host/server captured → localPlayer = host data
- `false`: Client captured → localPlayer = client data

Used during restoration to correctly map players.

## Restoration

Snapshot can restore:
- Health and mana values
- Cards in hand (instantiated from CardLibrary)
- Cards on board with correct positions
- Card status flags (tapped, summoning sickness)

Cannot restore:
- Exact card GameObject references
- Visual animation states
- Pending actions/timers

---
*Parent: [Networking System](./README.md)*
