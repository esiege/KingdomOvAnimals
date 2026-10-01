# NetworkBoardState

`Assets/Scripts/Network/NetworkBoardState.cs` (singleton, `NetworkBoardState.Instance`) — the single source of
truth for all gameplay state. Server-authoritative: every mutation happens inside a `[ServerRpc]` on this class,
after re-validating turn ownership, mana, slot occupancy, and target legality against the current `State.Value`.

## Synced State

```csharp
public readonly SyncVar<BoardState> State;
```

One `SyncVar` holding the entire `BoardState` (see [architecture.md](../architecture.md#model)). Clients never
mutate it directly — they call `Cmd*` methods and react to events.

## Server Commands (`[ServerRpc(RequireOwnership = false)]`)

| Command | Purpose |
|---------|---------|
| `InitializeGame(deck0, deck1)` *(not an RPC — called server-side by `NetworkGameManager`)* | Shuffles both decks, draws 4-card opening hands, calls `BoardState.StartGame()` |
| `CmdPlayCard(playerId, handIndex, slotIndex)` | Move a hand card to an empty board slot, spend mana |
| `CmdUseFlipAbility(attackerPlayerId, handIndex, targetPlayerId, targetSlotIndex)` | Hand card's offensive ability vs. a board card, spend mana |
| `CmdUseFlipAbilityOnPlayer(attackerPlayerId, handIndex, targetPlayerId)` | Hand card's offensive ability vs. the opponent directly |
| `CmdUseBoardAbility(attackerPlayerId, attackerSlotIndex, targetPlayerId, targetSlotIndex)` | Board card attacks a board card, taps attacker |
| `CmdAttackPlayer(attackerPlayerId, attackerSlotIndex, targetPlayerId)` | Board card attacks the opponent directly, taps attacker |
| `CmdUseSupportAbility(sourcePlayerId, sourceSlotIndex, targetPlayerId, targetSlotIndex)` | Board card's defensive ability on a **friendly** board card only (rejects if `targetPlayerId != sourcePlayerId`) |
| `CmdHealPlayer(sourcePlayerId, sourceSlotIndex)` | Board card's defensive ability heals its own player directly |
| `CmdEndTurn(playerId)` | Advances `BoardState.CurrentTurnPlayerId`/`TurnNumber` **and draws a card** for the new current player |

Damage/heal amounts and target types come from the acting card's `AbilityData` (`offensiveAbility` for attacks,
`defensiveAbility` for support), looked up via the `CardLibrary` reference wired on this component.

## Events (client-side)

`OnStateChanged(BoardState)`, `OnCardPlayed`, `OnCardDamaged`, `OnCardDied`, `OnCardDrawn`, `OnTurnChanged`,
`OnPlayerDamaged`, `OnPlayerHealed`, `OnSupportAbilityUsed`, `OnGameEnded` — all fired from `[ObserversRpc]`
methods after the server commits a state change. `BoardView`/`HandView` subscribe to these (mainly
`OnStateChanged`) to re-render.

## Queries

`GetState()`, `GetPlayerState(playerId)`, `IsPlayerTurn(playerId)`, `GetCurrentTurnPlayerId()`.

## Debugging

`BUILD_STAMP` (a `YYYYMMDD_HHMM` constant) logs on `Awake`/`OnStartServer`/`OnStartClient` — bump it when
tracking down editor/build code mismatches (`SyncType not found for index X` errors). See
[CLAUDE.md](../../CLAUDE.md) for the log file locations.

---
*Parent: [Networking System](./README.md)*
