# NetworkGameManager

`Assets/Scripts/Network/NetworkGameManager.cs` (singleton, `NetworkGameManager.Instance`) — turn/connection
bookkeeping that sits *alongside* `NetworkBoardState`, not on top of it. Per its own header comment: "Story 036:
Simplified to work with NetworkBoardState architecture."

## Synced State

| SyncVar | Type | Purpose |
|---------|------|---------|
| `CurrentTurnObjectId` | int | ObjectId of the `NetworkPlayer` whose turn it is (-1 = not started) |
| `TurnNumber` | int | This manager's own turn counter — **separate from `BoardState.TurnNumber`**, see [Turn Synchronization](./turn-synchronization.md) |
| `GameStarted` | bool | Set once both players are registered and the game has been initialized |
| `ShuffleSeed` | int | Generated but not currently consumed by `NetworkBoardState.InitializeGame` (which shuffles independently with `UnityEngine.Random`) |
| `OpponentDisconnected` | bool | Drives the disconnect grace-period timer |

## Player Registration & Game Start

`RegisterNetworkPlayer(player)` is called from `NetworkPlayer.OnStartServer()`/`OnOwnershipClient()`. It
identifies local vs. opponent by `IsOwner`, and once both are registered (server-side), calls
`ServerStartGame()`, which:

1. Generates `ShuffleSeed`
2. Loads a `DeckData` from `Resources/Decks` (tries `DefaultDeck`, else the first deck found) and calls
   `NetworkBoardState.Instance.InitializeGame(...)` with the same deck for both players
3. Randomly picks the first player and sets `CurrentTurnObjectId`
4. Sets `GameStarted = true`, fires `RpcGameStarted`

## Turn Management

`RequestEndTurn()` → `CmdEndTurn(objectId)`: validates it's the requester's turn, finds the other registered
`NetworkPlayer`, increments `TurnNumber`, sets `CurrentTurnObjectId` to the next player, fires `RpcTurnChanged`.
**This method does not call into `NetworkBoardState`** — no card draw, no `BoardState.CurrentTurnPlayerId`
change happens here. See [Turn Synchronization](./turn-synchronization.md).

`IsLocalPlayerTurn()` compares `CurrentTurnObjectId` to the local `NetworkPlayer.ObjectId`.

## Disconnect Handling

`OnPlayerDisconnected(playerId)` sets `OpponentDisconnected = true` (as of vdate, not called from anywhere in
the codebase — see [Reconnection](./reconnection.md)). While `OpponentDisconnected` is true, `Update()` runs a
grace-period timer (`reconnectGracePeriod`, default 120s); on expiry it loads the `MainMenu` scene after a 3s
delay.

---
*Parent: [Networking System](./README.md)*
