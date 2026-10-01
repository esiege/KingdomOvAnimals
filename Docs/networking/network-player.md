# NetworkPlayer

`Assets/Scripts/Network/NetworkPlayer.cs` — one instance per connected player. Per its own header comment:
"Card play and ability logic has been moved to NetworkBoardState. NetworkPlayer is now identity-only plus basic
stats."

## Synced State

| SyncVar | Initial (sentinel) | Notes |
|---------|---------------------|-------|
| `PlayerId` | -1 | 0 = host's perspective "Player", 1 = "Opponent". Set from `Owner.ClientId` for new connections |
| `PlayerName` | `"__UNSET__"` | |
| `IsReady` | -1 | Stored as `int` (-1 unset, 0 false, 1 true) — see code comment on why (FishNet `WriteFull` skips SyncVars equal to their initial value, which broke serialization counts with a plain `bool`) |
| `CurrentHealth` / `MaxHealth` | -1 | Defaulted to 20/20 in `OnStartServer` for a new player |
| `CurrentMana` / `MaxMana` | -1 | Defaulted to 1/1 in `OnStartServer` for a new player |

These mirror `PlayerBoardState.Health`/`Mana` but are a **separate** set of values on a separate object — not
automatically kept in sync with `BoardState`. Don't assume reading `NetworkPlayer.CurrentHealth` gives you the
same number as `BoardState.GetPlayer(id).Health` without checking who last wrote to which.

## Server RPCs (basic stats only)

`CmdTakeDamage`, `CmdHeal`, `CmdSpendMana`, `CmdRefillMana`, `CmdIncreaseMaxMana` — all `[ServerRpc(RequireOwnership = false)]`.
None of these are currently called from `NetworkBoardState`'s `Cmd*` methods, which mutate `PlayerBoardState`
fields directly instead. Verify which system a given feature actually updates before relying on these.

## Reconnection Hooks

`SetPendingState(DisconnectedPlayerState)` (called before spawn) and the deprecated `RestoreFromState` restore
these SyncVars for a reconnecting player. `OnStartServer()` branches on whether pending state/ID was set
(reconnect path) vs. a brand-new connection. See [Reconnection](./reconnection.md) for whether anything
currently calls the reconnect path.

`ServerRestoreGameState(json)` is a no-op stub — `GameStateSnapshot` (JSON-based restore) was removed in Story
036 in favor of `NetworkBoardState`'s own SyncVar.

---
*Parent: [Networking System](./README.md)*
