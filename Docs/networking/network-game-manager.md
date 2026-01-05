# NetworkGameManager

Central orchestrator for networked game state. Manages turn synchronization, player linking, and disconnect handling.

## Responsibilities

- Links spawned `NetworkPlayer` objects to scene `PlayerController` objects
- Synchronizes turn state across all clients
- Detects and handles opponent disconnection
- Coordinates game initialization with deterministic seeding

## Scene Requirements

Must be placed in the DuelScreen scene with references to:
- `localPlayerController` - The "Player" PlayerController
- `opponentPlayerController` - The "Opponent" PlayerController  
- `encounterController` - Scene's EncounterController

## Synchronized State

| SyncVar | Type | Purpose |
|---------|------|---------|
| `CurrentTurnObjectId` | int | ObjectId of NetworkPlayer whose turn it is (-1 = not started) |
| `TurnNumber` | int | Turn counter starting at 1 |
| `GameStarted` | bool | Game has been initialized |
| `ShuffleSeed` | int | Random seed for deterministic deck shuffling |
| `OpponentDisconnected` | bool | An opponent has disconnected |

## Player Linking

When clients connect, `FindAndLinkPlayers()` coroutine:
1. Waits for NetworkPlayer objects to spawn
2. Identifies which NetworkPlayer is local vs opponent
3. Links NetworkPlayer to corresponding PlayerController
4. Sets up bidirectional references for state synchronization

## Turn Management

The server controls all turn state:

1. **Server sets `CurrentTurnObjectId`** to the active player's ObjectId
2. **All clients receive SyncVar callback**
3. Each client determines if it's their turn by comparing ObjectIds
4. Turn-start actions (mana refill, card draw) execute on the correct client

## Disconnect Handling

When a player disconnects:
1. Server sets `OpponentDisconnected = true`
2. 120-second grace period begins
3. UI shows countdown timer
4. If player reconnects, `OpponentDisconnected` clears
5. If grace period expires, opponent forfeits

## Key Methods

| Method | Context | Description |
|--------|---------|-------------|
| `OnStartClient()` | Client | Initiates player linking process |
| `StartGameServer()` | Server | Initializes game state and first turn |
| `EndTurnServer()` | Server | Advances to next turn |
| `OnTurnChanged()` | All | SyncVar callback for turn updates |
| `RestoreGameStateLocally()` | Client | Restores state after reconnection |

## Initialization Sequence

1. NetworkManager spawns NetworkGameManager (scene object)
2. Both clients run `OnStartClient()`
3. Each client links NetworkPlayers via coroutine
4. Server calls `StartGameServer()` when both players ready
5. Server generates shuffle seed and sets initial turn
6. `OnNetworkGameStarted()` fires on all clients

---
*Parent: [Networking System](./README.md)*
