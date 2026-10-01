# EndTurnController

`Assets/Scripts/Controllers/EndTurnController.cs` — the only class left in `Assets/Scripts/Controllers/`. Very
small: an `OnMouseDown` handler on a clickable sprite.

```csharp
private void OnMouseDown()
{
    if (NetworkGameManager.Instance == null) return;
    if (!NetworkGameManager.Instance.IsLocalPlayerTurn()) return;
    NetworkGameManager.Instance.RequestEndTurn();
}
```

`RequestEndTurn()` sends the local player's `ObjectId` to the server, which validates turn ownership and calls
`NetworkGameManager`'s own (private) `CmdEndTurn` → advances **its own** `TurnNumber`/`CurrentTurnObjectId`
SyncVars only.

**This does not touch `BoardState`.** `NetworkBoardState` has a separate, public `CmdEndTurn(playerId)` (called
by `InputController.EndTurn()`, not by this button) that advances `BoardState.CurrentTurnPlayerId`/`TurnNumber`
*and* draws a card for the new player. As of this writing these are two independent turn-tracking paths that
both exist in the codebase — verify which one(s) are actually wired to the end-turn UI before assuming turn
advancement and card draw happen together.

There is no UI enable/disable or waiting-panel logic left in this class; that behavior, if present, now lives in
the scene's UI bindings rather than in code.

---
*Parent: [Controllers/View](./README.md)*
