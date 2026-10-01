# Game Flow

*Verified against source: 2026-08-12 (vdate) — rewritten; the previous version described
`EncounterController.Start()`, which no longer exists.*

## Match Initialization

```
Both players connect and register (NetworkGameManager.RegisterNetworkPlayer)
       │
       ▼
NetworkGameManager.ServerStartGame()
       ├── Generate ShuffleSeed
       ├── Load a DeckData from Resources/Decks
       ├── NetworkBoardState.InitializeGame(deck, deck) — same deck for both players
       │     ├── Shuffle both decks (Fisher-Yates)
       │     ├── Draw 4 cards each for opening hands
       │     └── BoardState.StartGame() — IsGameActive = true, Mana = MaxMana = 1
       └── Randomly pick starting player, set CurrentTurnObjectId
```

## Turn Structure

There are two independent turn-tracking systems — see
[Turn Synchronization](../networking/turn-synchronization.md). The one that actually affects gameplay is
`NetworkBoardState`'s: ending a turn via `InputController.EndTurn()` → `NetworkBoardState.CmdEndTurn` runs
`BoardState.EndTurn()`, which for the new current player:

1. Recalculates `MaxMana` from the shared turn counter (increases roughly every other turn, capped at 10) and
   refills `Mana` to that value
2. Untaps all board cards and clears summoning sickness (`CardState.OnTurnStart()`)
3. Draws one card (skipped if hand is full or deck is empty)

### During Turn

See [Targeting](./targeting.md) for the current click-based play/attack/support scheme.

### End of Turn

Handled server-side inside `CmdEndTurn` — see above. Client UI (`EndTurnController`) currently calls a
*different*, disconnected end-turn path (`NetworkGameManager.RequestEndTurn`) — see
[Turn Synchronization](../networking/turn-synchronization.md) before assuming the end-turn button drives the
turn transition described here.

## Win Conditions

`BoardState.CheckGameOver()`, called after any action that deals player damage: if either player's `Health`
reaches 0, `WinnerPlayerId` is set and `IsGameActive = false`. `NetworkBoardState` fires `OnGameEnded`.

---
*Parent: [Game Design](./README.md)*
