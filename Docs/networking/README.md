# Networking System

*Verified against source: 2026-08-12 (vdate)*

Kingdom Ov Animals uses **FishNet**. Server-authoritative: only the server mutates state, clients read via
SyncVars and events.

> This folder previously described `PlayerConnectionHandler` and `GameStateSnapshot` — neither class exists
> anymore. Reconnection now leans on `NetworkBoardState`'s own `SyncVar<BoardState>` instead of a hand-rolled
> JSON snapshot. See [Reconnection](./reconnection.md) for current (partial/WIP) status.

## Components

| Component | Purpose |
|-----------|---------|
| [`NetworkBoardState`](./network-board-state.md) | Single source of truth for cards/hands/mana/health — all game rules execute here |
| [`NetworkGameManager`](./network-game-manager.md) | Turn-order bookkeeping (separate from BoardState's own turn tracking — see below), player registration, disconnect grace period |
| [`NetworkPlayer`](./network-player.md) | Per-connection identity + basic stats (health/mana mirror) |
| `ConnectionManager` | Thin wrapper over FishNet's start/stop host/server/client |
| `MainMenuController` | Matchmaking: try to join, fall back to hosting after a timeout |
| [Reconnection](./reconnection.md) | `ReconnectionManager` + `DisconnectedPlayerState` — scaffolded, not fully wired as of vdate |

## ⚠️ Two turn-tracking systems

There are currently **two independent turn counters**:

1. `NetworkGameManager.CurrentTurnObjectId` / `TurnNumber` — ObjectId-based, driven by `EndTurnController`
2. `NetworkBoardState`'s `BoardState.CurrentTurnPlayerId` / `TurnNumber` — playerId-based, driven by
   `InputController.EndTurn()`, and the one that actually draws a card and resets board state on turn change

See [Turn Synchronization](./turn-synchronization.md) for details. Check which path a given UI element actually
calls before assuming turn-end behavior.

## Client-Server Communication

```
Client Input → ServerRpc (Cmd*) → Server validates against BoardState → State mutated → ObserversRpc (Rpc*) + C# event → All Clients re-render
```

See [architecture.md](../architecture.md#network) for the full `Cmd*`/`Rpc*` list.

---
*See also: [Troubleshooting](../troubleshooting/README.md)*
