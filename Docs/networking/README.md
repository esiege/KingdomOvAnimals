# Networking System

Kingdom Ov Animals uses **FishNet** for multiplayer functionality. The networking layer handles player connections, game state synchronization, turn management, and reconnection support.

## Architecture Overview

The networking system consists of five core components:

| Component | Purpose |
|-----------|---------|
| `NetworkGameManager` | Game state orchestration and turn synchronization |
| `PlayerConnectionHandler` | Connection lifecycle and player spawning |
| `NetworkPlayer` | Per-player state synchronization (health, mana) |
| `MainMenuController` | Matchmaking and lobby management |
| `GameStateSnapshot` | State serialization for reconnection |

## Connection Flow

1. **First player** clicks "Find Match" → attempts to join existing game
2. After 3-second timeout, becomes **host** (server + client)
3. **Second player** clicks "Find Match" → successfully connects as client
4. When both players connect, host loads DuelScreen scene globally
5. `NetworkGameManager` links `NetworkPlayer` objects to local `PlayerController` objects

## State Synchronization

All networked state uses FishNet SyncVars with automatic client notification:

**NetworkGameManager SyncVars:**
- `CurrentTurnObjectId` - Which NetworkPlayer's turn
- `TurnNumber` - Current turn count
- `GameStarted` - Game initialization flag
- `ShuffleSeed` - Deterministic deck shuffling
- `OpponentDisconnected` - Disconnect detection flag

**NetworkPlayer SyncVars:**
- `PlayerId` - Connection identifier
- `PlayerName` - Display name
- `CurrentHealth` / `MaxHealth`
- `CurrentMana` / `MaxMana`
- `IsReady` - Ready status

## Client-Server Communication

Actions flow through ServerRpc (client→server) and ObserversRpc (server→all clients):

```
Client Input → ServerRpc → Server Validation → State Change → ObserversRpc → All Clients
```

Example commands:
- `CmdPlayCard(cardIndex, slotIndex)` - Play card from hand
- `CmdUseAbilityOnCard(attackerSlot, targetSlot, isOffensive)` - Use ability
- `CmdEndTurn()` - End current turn
- `CmdRefillMana()` - Refill mana (turn start)

## Documentation Index

- [NetworkGameManager](./network-game-manager.md) - Core game state management
- [PlayerConnectionHandler](./player-connection-handler.md) - Connection lifecycle
- [NetworkPlayer](./network-player.md) - Per-player synchronization
- [Turn Synchronization](./turn-synchronization.md) - Turn management details
- [Reconnection System](./reconnection.md) - Disconnect handling
- [State Snapshot](./state-snapshot.md) - Game state serialization

---
*See also: [Troubleshooting](../troubleshooting/README.md)*
