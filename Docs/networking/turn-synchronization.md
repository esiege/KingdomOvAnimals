# Turn Synchronization

Turn management in multiplayer uses server-authoritative state with client-side visual synchronization.

## Turn State

Stored in NetworkGameManager as SyncVars:

- `CurrentTurnObjectId` - ObjectId of the NetworkPlayer whose turn it is
- `TurnNumber` - Sequential turn counter (starts at 1)

## Turn Flow

### Server Side (Host)

1. **Game Start**: Server randomly picks first player, sets `CurrentTurnObjectId`
2. **End Turn**: Server increments `TurnNumber`, swaps `CurrentTurnObjectId`
3. All changes automatically sync via SyncVars

### Client Side (All)

1. **SyncVar Callback**: `OnTurnChanged()` fires when `CurrentTurnObjectId` changes
2. **Determine Turn**: Compare `CurrentTurnObjectId` with local NetworkPlayer's `ObjectId`
3. **Execute Start**: If it's local player's turn, run turn-start logic

## Turn Start Actions

When turn changes to local player:

| Action | Description |
|--------|-------------|
| Refill Mana | Restore mana to max via `CmdRefillMana()` |
| Draw Card | Draw one card from deck |
| Reset Board | Clear tapped/summoning sickness flags |
| Update UI | Highlight playable cards |

These actions are **only executed by the player whose turn it is**.

## First Turn Handling

Turn 1 is special - no mana refill or card draw (players drew initial hands during setup). The first turn only enables card interactions for the first player.

## End Turn

1. Player clicks End Turn button
2. `CmdEndTurn()` ServerRpc fires
3. Server validates it's actually their turn
4. Server calls `NetworkGameManager.EndTurnServer()`
5. Server updates `TurnNumber` and `CurrentTurnObjectId`
6. SyncVar callbacks fire on all clients
7. Next player's turn starts

## Local Turn Check

```
bool IsLocalPlayerTurn = (NetworkGameManager.CurrentTurnObjectId == localPlayer.ObjectId)
```

Used for:
- Validating client actions before sending to server
- Enabling/disabling UI interactions
- Determining which player runs turn-start logic

## Edge Cases

- **Disconnect during turn**: Turn continues for remaining player after grace period
- **Reconnection**: Turn state restored from snapshot, continues from last known state
- **Simultaneous actions**: Server serializes all actions, only processes one at a time

---
*Parent: [Networking System](./README.md)*
