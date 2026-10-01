# Reconnection

*Verified against source: 2026-08-12 (vdate)*

> This doc previously described `PlayerConnectionHandler` and `GameStateSnapshot`. **Neither class exists in
> the codebase anymore.** The design docs below (and [reconnection_analysis.md](../reconnection_analysis.md))
> proposed replacing the old snapshot-based approach with a despawn/respawn pattern built on
> `NetworkBoardState`'s SyncVar — that direction was taken, but as of this writing it is **scaffolded, not
> fully wired end-to-end**. The most recent commit on this branch is literally titled "netcode wip".

## What exists today

| Piece | File | Status |
|-------|------|--------|
| Disconnect flag + grace timer | `NetworkGameManager.OpponentDisconnected`, `Update()` grace-period loop | Wired — timer runs and returns to `MainMenu` on expiry |
| Trigger for that flag | `NetworkGameManager.OnPlayerDisconnected(playerId)` | **Not called anywhere** in the codebase as of vdate — nothing currently sets `OpponentDisconnected` from an actual FishNet disconnect event |
| State capture | `DisconnectedPlayerState.Capture(NetworkPlayer)` | Defined, reads health/mana from `NetworkPlayer` and hand/board/deck from `NetworkBoardState` — **not called anywhere** as of vdate |
| Reconnect attempt loop | `ReconnectionManager` (static class, `Assets/Scripts/Network/ReconnectionManager.cs`) | Defined (editor-update-driven retry every 3s, 120s grace period) — `StartReconnectionWait()` **has no call sites** as of vdate |
| Restore-on-respawn | `NetworkPlayer.SetPendingState(DisconnectedPlayerState)` / `OnStartServer()` reconnect branch | Defined, consumes a `DisconnectedPlayerState` if one was set before spawn — nothing currently calls `SetPendingState` |

In short: the pieces for a despawn/respawn reconnection flow are in place, but no code path currently connects
"a client actually disconnected" to any of them. Don't assume reconnection works — check current call sites
(`grep` for the method names above) before relying on this for testing or new work.

## Design history

- [reconnection_analysis.md](../reconnection_analysis.md) — the redesign proposal that led to the current
  `DisconnectedPlayerState`/`ReconnectionManager` shape (historical; kept for rationale, not as a guide to
  current behavior)
- [troubleshooting/reconnection-debugging.md](../troubleshooting/reconnection-debugging.md) and
  [troubleshooting/turn-sync-issues.md](../troubleshooting/turn-sync-issues.md) — dated incident logs from the
  *previous* (`PlayerConnectionHandler`/`EncounterController`) implementation; useful for the failure patterns
  they describe (duplicate turn actions, singleton destruction races), not for current class names

---
*Parent: [Networking System](./README.md)*
