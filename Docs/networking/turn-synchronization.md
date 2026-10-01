# Turn Synchronization

*Verified against source: 2026-08-12 (vdate)*

There are **two independent turn-tracking systems** currently in the codebase. This is not a doc error — both
exist in source. Confirm which one a given code path actually drives before changing turn logic.

## System 1: `NetworkGameManager` (ObjectId-based)

- SyncVars: `CurrentTurnObjectId`, `TurnNumber`
- Advanced by: `EndTurnController.OnMouseDown()` → `NetworkGameManager.RequestEndTurn()` → `CmdEndTurn`
- Effect: swaps `CurrentTurnObjectId` to the other registered `NetworkPlayer`, increments `TurnNumber`. **Does
  not draw a card, does not touch `BoardState`.**
- `IsLocalPlayerTurn()` compares `CurrentTurnObjectId` to the local `NetworkPlayer.ObjectId`.

## System 2: `NetworkBoardState` (playerId-based)

- Fields (inside the `BoardState` SyncVar): `CurrentTurnPlayerId`, `TurnNumber`
- Advanced by: `InputController.EndTurn()` → `NetworkBoardState.Instance.CmdEndTurn(LocalPlayerId)`
- Effect: `BoardState.EndTurn()` swaps `CurrentTurnPlayerId`, increments `TurnNumber` when play returns to
  player 0, calls `PlayerBoardState.OnTurnStart()` (untaps cards, clears sickness, recalculates `MaxMana`) for
  the new current player, **and draws that player a card**.
- `NetworkBoardState.IsPlayerTurn(playerId)` — this is what `InputController` actually checks before accepting
  input (`IsMyTurn()`), not `NetworkGameManager.IsLocalPlayerTurn()`.

## Practical implication

`InputController` (the thing players actually interact with) gates on **System 2**. `EndTurnController`'s
button drives **System 1**. Whether a given UI's end-turn button and the board's actual turn state stay in sync
depends on which of these two paths is actually wired to it in the scene — check both before debugging a
"turn didn't advance" or "card wasn't drawn" issue.

---
*Parent: [Networking System](./README.md)*
