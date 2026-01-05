# Story 010: Add Reconnection Support

## Status: In Progress
## Sprint: 01
## Started: 2026-01-04

---

## User Story

**As a** player  
**I want** to be able to reconnect to a game if I disconnect  
**So that** temporary network issues don't forfeit my match

---

## Acceptance Criteria

1. ✅ When a player disconnects, their NetworkPlayer is preserved (not destroyed)
2. ✅ Reconnecting player is matched back to their existing game session
3. ✅ Game state (health, mana, board, hands) is restored for reconnected player
4. ✅ Opponent sees "Opponent reconnected!" message
5. ✅ Game resumes from where it left off

---

## Technical Notes

### Current State (Story 009)
- Disconnect detected via `PlayerConnectionHandler.OnRemoteConnectionState`
- `NetworkGameManager.OpponentDisconnected` SyncVar notifies clients
- 30-second grace period before forfeit
- `ServerOnPlayerDisconnected(playerId)` and `ServerOnPlayerReconnected(playerId)` exist but reconnect not implemented

### Implementation Plan

1. **Preserve NetworkPlayer on disconnect** (don't despawn)
   - Modify PlayerConnectionHandler to NOT remove from tracking on disconnect
   - Store disconnected player's connection mapping
   
2. **Session matching on reconnect**
   - When player connects, check if they have an existing session
   - Re-assign their NetworkPlayer to their new connection
   
3. **State restoration**
   - NetworkPlayer SyncVars (health, mana) auto-sync on ownership change
   - Board state syncs via existing card positions
   - Hand state may need special handling

### Key Challenge
FishNet automatically despawns objects owned by disconnected clients. We need to either:
- **Option A**: Change NetworkPlayer ownership to server on disconnect, transfer back on reconnect
- **Option B**: Store game state separately and recreate on reconnect

**Chosen approach**: Option A (ownership transfer) - simpler, uses existing SyncVars

---

## Files to Modify

- `PlayerConnectionHandler.cs` - Preserve disconnected player, handle reconnect
- `NetworkGameManager.cs` - Session tracking, reconnect logic
- `NetworkPlayer.cs` - May need ownership transfer helpers

---

## Testing Checklist

- [ ] Client disconnects, reconnects within 30s - game resumes
- [ ] Client disconnects, reconnects after 30s - forfeit (existing behavior)
- [ ] Host disconnects - client returns to menu (no reconnect for host)
- [ ] Multiple disconnect/reconnect cycles work

---

## Notes

- Only client reconnection supported (host disconnect = game over)
- Player must reconnect from same game session (no cross-session)
