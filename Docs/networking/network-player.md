# NetworkPlayer

Represents a single connected player in the network. Spawned by server, one per client. Contains all synced game state for that player.

## Responsibilities

- Stores and synchronizes player health/mana
- Links to local PlayerController for UI updates
- Receives and validates client commands (ServerRpc)
- Broadcasts state changes to all clients (ObserversRpc)

## Synchronized State

All state uses FishNet SyncVars with change callbacks:

| SyncVar | Type | Default | Description |
|---------|------|---------|-------------|
| `PlayerId` | int | 0 | Connection identifier (0=host, 1=client) |
| `PlayerName` | string | "" | Display name |
| `IsReady` | bool | false | Ready to start game |
| `CurrentHealth` | int | 20 | Current HP |
| `MaxHealth` | int | 20 | Maximum HP |
| `CurrentMana` | int | 1 | Current mana pool |
| `MaxMana` | int | 1 | Maximum mana pool |

## Local References

| Property | Type | Description |
|----------|------|-------------|
| `LinkedPlayerController` | PlayerController | Scene controller this NetworkPlayer drives |
| `OnStateChanged` | Action | Event for UI update triggers |

## Server Commands (ServerRpc)

These methods validate and execute player actions:

| Command | Parameters | Description |
|---------|------------|-------------|
| `CmdPlayCard` | cardIndex, slotIndex | Play card from hand to board |
| `CmdUseAbilityOnCard` | attackerSlot, targetSlot, isOffensive | Use board card ability |
| `CmdUseFlipAbilityOnCard` | handIndex, targetSlot, isOffensive | Use hand card flip ability |
| `CmdEndTurn` | none | End current turn |
| `CmdRefillMana` | none | Refill mana at turn start |

## Client Broadcasts (ObserversRpc)

These methods synchronize state to all clients:

| RPC | Purpose |
|-----|---------|
| `RpcExecuteCardPlay` | All clients execute card placement |
| `RpcExecuteAbility` | All clients execute ability effect |

## State Update Flow

1. Client calls ServerRpc (e.g., `CmdPlayCard`)
2. Server validates action (turn, mana, slot availability)
3. Server updates SyncVars (mana deduction)
4. Server calls ObserversRpc for visual execution
5. All clients update their game state

## Change Callbacks

Each SyncVar has a corresponding `OnXChanged` callback:

- `OnPlayerIdChanged` - Logs player ID assignment
- `OnHealthChanged` - Updates PlayerController.currentHealth and UI
- `OnManaChanged` - Updates PlayerController.currentMana and UI
- etc.

These callbacks fire on all clients whenever the server changes a SyncVar value.

## Player Identification

Each NetworkPlayer has a unique `ObjectId` (FishNet assigned). The current turn is tracked by storing `CurrentTurnObjectId` in NetworkGameManager. Clients check `IsLocalPlayerTurn` by comparing:

```
IsLocalPlayerTurn = (gameManager.CurrentTurnObjectId == this.ObjectId)
```

---
*Parent: [Networking System](./README.md)*
