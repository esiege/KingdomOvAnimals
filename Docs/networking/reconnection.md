# Reconnection System

Handles player disconnection with a grace period for reconnection, preserving game state including health, mana, cards, and turn state.

## Overview

When a player disconnects:
- 120-second grace period begins
- Disconnected player can rejoin and restore state
- If grace period expires, disconnected player forfeits
- Remaining player wins

## Architecture

The reconnection system uses a **Despawn/Respawn** pattern:
1. On disconnect → Server captures state → NetworkPlayer despawned
2. On reconnect → Fresh NetworkPlayer spawned → State restored from snapshot

### Key Components

| Component | Responsibility |
|-----------|----------------|
| `NetworkGameManager` | Orchestrates state capture, restoration, and player tracking |
| `PlayerConnectionHandler` | Manages connection state and reconnection attempts |
| `GameStateSnapshot` | Serializable container for complete game state |
| `DisconnectedPlayerState` | Temporary storage of player state during disconnect |
| `EncounterController` | Handles turn state and prevents duplicate effects |

## Two Reconnection Scenarios

### Scenario A: Client Disconnects (Host Remains)
1. Server detects disconnect via `OnRemoteConnectionState`
2. Server captures `DisconnectedPlayerState` (including `wasTheirTurn` flag)
3. Server despawns the client's NetworkPlayer
4. Server sets `OpponentDisconnected = true`
5. If client reconnects:
   - Server spawns fresh NetworkPlayer with new ObjectId
   - Server removes OLD ObjectId from `networkPlayers` dictionary
   - Server updates `CurrentTurnObjectId` if it was their turn
   - Server broadcasts fresh turn state RPC to update buffer
   - Server sends `GameStateSnapshot` via `TargetReceiveGameState`

### Scenario B: Host Disconnects (Client Remains)
1. Client detects disconnect via `OnClientConnectionState`
2. Client saves local game state snapshot
3. Client attempts reconnection every 3 seconds
4. If host returns, client restores state locally

## GameStateSnapshot

Serializable snapshot of entire game state:

| Field | Type | Description |
|-------|------|-------------|
| `turnNumber` | int | Current turn number |
| `currentTurnObjectId` | int | ObjectId of player whose turn it is |
| `shuffleSeed` | int | Original deck shuffle seed |
| `localPlayer` | PlayerSnapshot | Capturing player's state |
| `opponent` | PlayerSnapshot | Opponent's state |
| `isFromServerPerspective` | bool | True if captured by host |
| `timestamp` | long | Unix timestamp for validation |

### PlayerSnapshot Contents

| Field | Type | Description |
|-------|------|-------------|
| `playerId` | int | Player identifier (0 or 1) |
| `health` | int | Current health |
| `maxHealth` | int | Maximum health |
| `mana` | int | Current mana |
| `maxMana` | int | Maximum mana |
| `handCardIds` | List\<string\> | Card names in hand |
| `boardCards` | List\<BoardCardSnapshot\> | Cards on board with slots |
| `deckCardIds` | List\<string\> | Remaining deck cards in order |
| `graveyardCardIds` | List\<string\> | Cards in graveyard |

## Critical Implementation Details

### 1. ObjectId Management

When a player reconnects, they get a **new ObjectId** (fresh NetworkPlayer spawn). The system must:

```csharp
// Remove OLD despawned NetworkPlayer from dictionary
if (savedState.oldObjectId > 0 && networkPlayers.ContainsKey(savedState.oldObjectId))
{
    networkPlayers.Remove(savedState.oldObjectId);
}

// Register the fresh NetworkPlayer
networkPlayers[newObjectId] = newNetworkPlayer;
```

### 2. Turn State Synchronization

The `CurrentTurnObjectId` SyncVar must be updated to the new ObjectId:

```csharp
if (savedState.wasTheirTurn)
{
    CurrentTurnObjectId.Value = newObjectId;
    
    // CRITICAL: Broadcast fresh turn state to update the buffered RPC
    RpcBroadcastTurnState(CurrentTurnObjectId.Value, TurnNumber.Value, GameStarted.Value, ShuffleSeed.Value);
}
```

### 3. Buffered RPC Handling

The `RpcBroadcastTurnState` uses `BufferLast = true`, which can send stale data to reconnecting clients. The fix:

1. **Server broadcasts fresh RPC** after updating `CurrentTurnObjectId`
2. **Client validates incoming RPCs** to prefer their local ObjectId over stale ones

```csharp
// Client-side validation in RpcBroadcastTurnState
if (turnNumber == _cachedTurnNumber && turnObjectId != _cachedTurnObjectId)
{
    // Prefer our local player's ObjectId if already cached
    if (localNetworkPlayer != null && _cachedTurnObjectId == localNetworkPlayer.ObjectId)
    {
        return; // Ignore stale RPC
    }
}
```

### 4. State Restoration Flag

To prevent duplicate mana/effects during reconnection, `EncounterController` uses:

```csharp
private bool isRestoringState = false;

// Set true in RestoreTurnState()
// Set false in OnGameStateRestored()
// Checked in StartTurnNetwork() to skip mana changes
```

### 5. Health/Mana Restoration

`RestoreCardsFromSnapshot` must restore HP/mana to `PlayerController`, not just cards:

```csharp
playerController.currentHealth = playerData.health;
playerController.maxHealth = playerData.maxHealth;
playerController.currentMana = playerData.mana;
playerController.maxMana = playerData.maxMana;
playerController.UpdatePlayerUI();
```

## State Restoration Flow

### Server → Client (TargetReceiveGameState)

```
1. Ensure CardLibrary is initialized
2. Call RestoreTurnState() [sets isRestoringState = true]
3. Call RestoreCardsFromSnapshot() [restores HP, mana, cards]
4. Call OnGameStateRestored() [sets isRestoringState = false]
```

### Preventing Duplicate Effects

The `OnNetworkGameStarted` guard prevents duplicate card draws:

```csharp
public void OnNetworkGameStarted(...)
{
    if (networkInitialized)
    {
        Debug.Log("[EncounterController] Already initialized - skipping");
        return;
    }
    networkInitialized = true;
    // ... proceed with initialization
}
```

## DisconnectedPlayerState

Temporary storage during disconnect grace period:

| Field | Type | Description |
|-------|------|-------------|
| `playerId` | int | Player identifier |
| `connection` | NetworkConnection | Original connection |
| `playerName` | string | Display name |
| `wasTheirTurn` | bool | Was it their turn when disconnected |
| `oldObjectId` | int | ObjectId before despawn (for cleanup) |
| `snapshot` | GameStateSnapshot | Full game state at disconnect |
| `disconnectTime` | float | Time.time when disconnected |

## Grace Period UI

EncounterController manages disconnect UI:

| Component | Purpose |
|-----------|---------|
| `disconnectPanel` | Overlay panel shown during disconnect |
| `disconnectStatusText` | Countdown timer display |
| `UpdateDisconnectTimer(float)` | Updates countdown text |
| `OnOpponentReconnected()` | Hides panel, resumes game |
| `OnOpponentForfeited()` | Shows victory message |

## Common Issues and Solutions

### Issue: Mana increases on reconnect
**Cause:** `StartTurnNetwork()` called during state restoration
**Solution:** `isRestoringState` flag prevents mana changes

### Issue: Turn doesn't transfer to reconnected player
**Cause:** Buffered RPC has old ObjectId
**Solution:** Server broadcasts fresh RPC after updating `CurrentTurnObjectId`

### Issue: Old player still in networkPlayers dictionary
**Cause:** Despawned NetworkPlayer not removed
**Solution:** Explicitly remove old ObjectId before adding new one

### Issue: Health/mana not restored
**Cause:** `RestoreCardsFromSnapshot` only restored cards
**Solution:** Also restore HP/mana to PlayerController

### Issue: Duplicate card draws after reconnect
**Cause:** `OnNetworkGameStarted` called again
**Solution:** Guard with `networkInitialized` flag

## Debug Logging

Key log messages to watch:

```
[Server] Removing OLD despawned NetworkPlayer {oldId} from networkPlayers dictionary
[Server] Registered fresh NetworkPlayer {newId} for player {id}. networkPlayers.Count=2
[Server] Updating CurrentTurnObjectId from {old} to {new}
[Server] Broadcasting fresh turn state to update buffered RPC
[EncounterController] Skipping mana changes - restoring state from snapshot
[Client] Turn state restored first (to block OnNetworkGameStarted)
[Client] Game state restoration complete!
```

## Testing Checklist

When testing reconnection, verify:

- [ ] Health preserved correctly
- [ ] Mana preserved correctly (no +1 on reconnect)
- [ ] Cards in hand match pre-disconnect
- [ ] Cards on board match pre-disconnect
- [ ] Turn indicator shows correct player
- [ ] Reconnected player can take actions if their turn
- [ ] Other player can end turn normally
- [ ] No duplicate card draws

---
*Parent: [Networking System](./README.md)*
