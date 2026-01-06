# GameStateSnapshot

Serializable representation of complete game state for persistence and reconnection.

## Purpose

- Capture game state when disconnection occurs
- Serialize to JSON for network transfer
- Restore state after reconnection (including health, mana, cards, turn state)
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
│   ├── maxHealth: int          ← Added to support proper HP restoration
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

### PlayerSnapshot Capture Priority

Values are captured with this priority:
1. **NetworkPlayer SyncVars** (if spawned and valid)
2. **PlayerController local values** (fallback)

This ensures accurate values even if NetworkPlayer is being despawned.

```csharp
// Capture priority example
if (controller.networkPlayer != null && controller.networkPlayer.IsSpawned)
{
    capturedHealth = controller.networkPlayer.CurrentHealth.Value;
    capturedMaxHealth = controller.networkPlayer.MaxHealth.Value;
    // ...
}
else
{
    capturedHealth = controller.currentHealth;
    capturedMaxHealth = controller.maxHealth;
    // ...
}
```

## Serialization

Uses Unity's `JsonUtility`:

```csharp
snapshot.ToJson() → JSON string
GameStateSnapshot.FromJson(json) → GameStateSnapshot
```

## Perspective Flag

`isFromServerPerspective` indicates who captured the snapshot:
- `true`: Host/server captured → localPlayer = host data
- `false`: Client captured → localPlayer = client data

Used during restoration to correctly map players.

## Restoration

### What Gets Restored

| Data | Restored To | Notes |
|------|-------------|-------|
| `health` | PlayerController.currentHealth | Also updates UI |
| `maxHealth` | PlayerController.maxHealth | Needed for HP bar |
| `mana` | PlayerController.currentMana | Also updates UI |
| `maxMana` | PlayerController.maxMana | Needed for mana display |
| `handCardIds` | Hand via CardLibrary lookup | Instantiated fresh |
| `boardCards` | Board slots | With full card state |
| `deckCardIds` | PlayerController.deck | Order preserved |
| `graveyardCardIds` | PlayerController.graveyard | Order preserved |

### Restoration Code Path

```
TargetReceiveGameState (RPC)
  └── RestoreTurnState()        ← Sets isRestoringState = true
  └── RestoreCardsFromSnapshot()
        ├── Restore HP/mana to PlayerController
        ├── Restore hand cards
        ├── Restore board cards
        └── Restore deck/graveyard
  └── OnGameStateRestored()     ← Sets isRestoringState = false
```

### Critical: Mana Restoration

The `isRestoringState` flag in `EncounterController` prevents duplicate mana increases:

```csharp
// In StartTurnNetwork()
if (isRestoringState)
{
    Debug.Log("Skipping mana changes - restoring state from snapshot");
    return; // Don't add +1 mana during restoration
}
```

## Cannot Restore

- Exact card GameObject references (new instances created)
- Visual animation states
- Pending actions/timers
- Transient UI states

## Related Files

- [GameStateSnapshot.cs](../../Assets/Scripts/Network/GameStateSnapshot.cs)
- [NetworkGameManager.cs](../../Assets/Scripts/Network/NetworkGameManager.cs) - `RestoreCardsFromSnapshot()`
- [EncounterController.cs](../../Assets/Scripts/Controllers/EncounterController.cs) - `RestoreTurnState()`, `OnGameStateRestored()`

---
*Parent: [Networking System](./README.md)*
