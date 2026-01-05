# PlayerConnectionHandler

Manages player connection lifecycle including spawning, tracking, and cleanup. Singleton that persists across scene loads.

## Responsibilities

- Spawns `NetworkPlayer` prefab for each connecting client
- Tracks connected players by connection ID
- Manages disconnected player state for reconnection
- Handles client-side reconnection attempts when host disconnects

## Lifecycle

```
Client Connects → OnRemoteConnectionState(Started) → SpawnPlayerObject()
                                                          ↓
                                            NetworkPlayer spawned with ownership
                                                          ↓
                                            Player added to ConnectedPlayers dict
```

## Server-Side: Player Spawning

When a client connects:
1. `OnRemoteConnectionState` fires with `RemoteConnectionState.Started`
2. Creates new `NetworkPlayer` instance from prefab
3. Assigns player ID (0 = host, 1 = client)
4. Spawns with ownership given to connecting client
5. Stores reference in `ConnectedPlayers` dictionary

## Client-Side: Reconnection

When host disconnects (client perspective):
1. `OnClientConnectionState(Stopped)` fires
2. `OnHostDisconnected()` saves game state snapshot
3. Sets `_isWaitingForHostReconnect = true`
4. Records server address/port for reconnection
5. Shows disconnect UI via EncounterController

Reconnection attempts:
- Tries every 3 seconds (`_reconnectAttemptInterval`)
- 120-second grace period (`_hostReconnectGracePeriod`)
- On success, `OnReconnectedToHost()` restores state
- On timeout, returns to main menu

## Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | static | Singleton reference |
| `ConnectedPlayers` | Dictionary | Active players by connection ID |
| `PlayerCount` | int | Current connected player count |

## Key Methods

| Method | Context | Description |
|--------|---------|-------------|
| `OnRemoteConnectionState()` | Server | Handles client connect/disconnect |
| `OnClientConnectionState()` | Client | Handles own connection state changes |
| `OnHostDisconnected()` | Client | Initiates reconnection wait |
| `AttemptReconnect()` | Client | Tries to reconnect to host |
| `OnReconnectedToHost()` | Client | Handles successful reconnection |
| `ForfeitDisconnectedPlayer()` | Server | Cleans up forfeited player |

## Reconnection State

| Field | Purpose |
|-------|---------|
| `_isWaitingForHostReconnect` | Currently waiting for host |
| `_hostReconnectTimer` | Time elapsed since disconnect |
| `_savedGameState` | GameStateSnapshot for restoration |
| `_lastServerAddress` | Host IP for reconnection |
| `_lastServerPort` | Host port for reconnection |

## Debugging

Writes to separate log file `Docs/reconnect_editor.txt` or `reconnect_build.txt` for reconnection debugging independent of Unity's logging system.

---
*Parent: [Networking System](./README.md)*
