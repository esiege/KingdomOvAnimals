# Reconnection System Analysis & Redesign Proposal

## Current Problems
After a client disconnects and reconnects:
1. Mana shows as 0 instead of the correct value
2. Card draw doesn't work (LinkedPlayerController is null)
3. Cards are out of sync between client and server
4. Client can't find their own NetworkPlayer after reconnection

---

## Attempted Fixes (Chronological)

### Attempt 1: Fix Mana Capture in Snapshot
**File:** `NetworkGameManager.cs` - `CapturePlayerSnapshot()`

**Problem:** Server was capturing `controller.currentMana` (local value) instead of the authoritative `controller.networkPlayer.CurrentMana.Value`.

**Fix:** Changed to prefer NetworkPlayer SyncVar values:
```csharp
// Before
capturedMana = controller.currentMana;

// After
if (controller.networkPlayer != null && controller.networkPlayer.IsSpawned)
{
    capturedMana = controller.networkPlayer.CurrentMana.Value;
}
```

**Outcome:** ❌ Mana still 0 after reconnection - root cause was elsewhere.

---

### Attempt 2: Remove BufferLast from Card Draw RPC
**File:** `NetworkPlayer.cs` - `RpcExecuteCardDraw()`

**Problem:** `[ObserversRpc(BufferLast = true)]` was causing buffered RPCs to fire before state was restored.

**Fix:** Changed to `[ObserversRpc]` (no buffering).

**Outcome:** ⚠️ Partial - Reduced errors but draw still fails because LinkedPlayerController is null.

---

### Attempt 3: Enhanced TryFindLocalPlayer with ClientId Check
**File:** `NetworkGameManager.cs` - `TryFindLocalPlayer()`

**Problem:** Player search only checked `IsOwner`, which doesn't work immediately after reconnection.

**Fix:** Added fallback to check `player.Owner.ClientId == localClientId`:
```csharp
bool isLocalPlayer = player.IsOwner;
if (!isLocalPlayer && player.Owner != null && localClientId >= 0)
{
    isLocalPlayer = (player.Owner.ClientId == localClientId);
}
```

**Outcome:** ❌ Still fails - the NetworkPlayer isn't visible to the client at all.

---

### Attempt 4: Add Cooldown to Reduce Error Spam
**File:** `NetworkGameManager.cs` - `IsLocalPlayerTurn()`

**Problem:** Console flooded with "Could not find localNetworkPlayer" errors.

**Fix:** Added 0.5s cooldown between error logs.

**Outcome:** ✅ Reduced spam, but underlying issue remains.

---

### Attempt 5: Coroutine Retry for NetworkPlayer Registration
**File:** `NetworkGameManager.cs` - `ReRegisterNetworkPlayersWithRetry()`

**Problem:** NetworkPlayer registration runs immediately but ownership hasn't propagated yet.

**Fix:** Created coroutine that retries 30 times (3 seconds) waiting for both players:
```csharp
private IEnumerator ReRegisterNetworkPlayersWithRetry()
{
    for (int attempt = 1; attempt <= 30; attempt++)
    {
        ReRegisterNetworkPlayers();
        if (localNetworkPlayer != null && opponentNetworkPlayer != null)
            yield break; // Success
        yield return new WaitForSeconds(0.1f);
    }
}
```

**Outcome:** ❌ All 30 attempts fail - client NEVER sees their NetworkPlayer.

---

### Attempt 6: RebuildObservers on Reconnection
**File:** `PlayerConnectionHandler.cs` - `HandleReconnection()`

**Problem:** `GiveOwnership()` alone doesn't make the NetworkObject visible to the new connection.

**Fix:** Added `RebuildObservers()` call after ownership transfer:
```csharp
player.GiveOwnership(conn);
_networkManager.ServerManager.Objects.RebuildObservers(networkObject, conn);
```

**Outcome:** ❌ Cards now out of sync - made things worse.

---

## Root Cause Analysis

The fundamental issue is that we're trying to **patch** a broken reconnection flow instead of **designing** a proper one.

### Current Flow (Broken):
```
1. Client disconnects
2. Server preserves NetworkPlayer (doesn't despawn)
3. Client reconnects with NEW ClientId
4. Server calls GiveOwnership() + RebuildObservers()
5. Server sends TargetRpc with game state snapshot
6. Client tries to find NetworkPlayers via FindObjectsOfType
7. Client only sees OPPONENT's NetworkPlayer, not their own
8. Everything breaks
```

### Why It Fails:
- FishNet's ownership/observer system wasn't designed for "hot-swap" reconnection
- The client's NetworkPlayer object exists on server but may not be properly spawned for the new connection
- State restoration happens before the network layer is fully synchronized
- We're fighting against FishNet's architecture

---

## Proposed Redesign: Clean Slate Reconnection

Instead of trying to preserve the exact NetworkPlayer object, we should:

### Option A: Despawn/Respawn Pattern
```
1. Client disconnects
2. Server DESPAWNS their NetworkPlayer (but saves state to dictionary)
3. Client reconnects
4. Server creates FRESH NetworkPlayer for new connection
5. Server restores saved state to the new NetworkPlayer
6. Client receives fresh spawn with correct ownership
7. State restoration happens naturally
```

**Pros:**
- Works WITH FishNet's design, not against it
- Fresh spawn guarantees correct ownership
- No observer/visibility issues

**Cons:**
- Need to carefully preserve all state
- Brief moment where player "doesn't exist"

### Option B: Scene Reload Pattern
```
1. Client disconnects
2. Server saves full game state
3. Client reconnects
4. Server tells client to reload the DuelScreen scene
5. Scene loads fresh, server sends state
6. Everything initializes cleanly
```

**Pros:**
- Cleanest approach
- All objects initialize properly
- No weird ownership states

**Cons:**
- User sees loading screen
- All visual state (animations, etc.) lost

### Option C: NetworkPlayer Pool with Fresh Assignment
```
1. Pre-spawn 2 NetworkPlayers without owners
2. Assign ownership dynamically when players connect
3. On disconnect, REMOVE ownership (don't despawn)
4. On reconnect, RE-ASSIGN ownership to the same NetworkPlayer
5. Use SyncVars for all state (they auto-sync on ownership change)
```

**Pros:**
- Objects always exist
- Ownership changes are the only variable
- FishNet handles SyncVar sync automatically

**Cons:**
- Requires refactoring how NetworkPlayers are created
- More complex initial setup

---

## Recommended Approach: Option A (Despawn/Respawn)

This is the most pragmatic solution that works with FishNet's architecture.

### Implementation Plan:

#### Step 1: Create State Storage
```csharp
public class DisconnectedPlayerState
{
    public int PlayerId;
    public string PlayerName;
    public int Health;
    public int Mana;
    public int MaxMana;
    public List<string> HandCardIds;
    public List<BoardCardSnapshot> BoardCards;
    public List<string> DeckCardIds;
    public List<string> GraveyardCardIds;
    // Any other state...
}
```

#### Step 2: On Disconnect - Save & Despawn
```csharp
private void OnPlayerDisconnected(NetworkConnection conn)
{
    var player = GetPlayerByConnection(conn);
    if (player != null)
    {
        // Save state
        var state = CapturePlayerState(player);
        _disconnectedPlayerStates[player.PlayerId.Value] = state;
        
        // Despawn the NetworkPlayer
        _networkManager.ServerManager.Despawn(player.NetworkObject);
    }
}
```

#### Step 3: On Reconnect - Respawn & Restore
```csharp
private void OnPlayerReconnected(NetworkConnection conn, int playerId)
{
    // Spawn fresh NetworkPlayer
    var nob = _networkManager.GetPooledInstantiated(playerPrefab, true);
    var newPlayer = nob.GetComponent<NetworkPlayer>();
    
    // Restore state BEFORE spawning (so SyncVars are set)
    var savedState = _disconnectedPlayerStates[playerId];
    newPlayer.RestoreFromState(savedState);
    
    // Spawn with correct ownership
    _networkManager.ServerManager.Spawn(nob, conn);
    
    // Clean up saved state
    _disconnectedPlayerStates.Remove(playerId);
    
    // Notify game manager
    NetworkGameManager.Instance.OnPlayerRespawned(newPlayer, savedState);
}
```

#### Step 4: Client-Side - Wait for Fresh Spawn
The client doesn't need special reconnection logic! They just wait for their NetworkPlayer to spawn like normal, and the `OnStartClient` callback handles everything.

---

## Files That Need Modification

1. **PlayerConnectionHandler.cs**
   - Add `_disconnectedPlayerStates` dictionary
   - Modify `OnPlayerDisconnected` to save state & despawn
   - Modify `TryReconnectPlayer` to spawn fresh & restore

2. **NetworkPlayer.cs**
   - Add `RestoreFromState(DisconnectedPlayerState state)` method
   - Ensure all important state is in SyncVars

3. **NetworkGameManager.cs**
   - Remove all the bandaid retry/coroutine code
   - Simplify state restoration (let spawn handle it)
   - Add `OnPlayerRespawned` callback

4. **GameStateSnapshot.cs** (or new file)
   - Create `DisconnectedPlayerState` class
   - Add serialization helpers

---

## Benefits of This Approach

1. **Works WITH FishNet** - Uses standard spawn/despawn flow
2. **Clean ownership** - New spawn = correct owner from the start
3. **SyncVars just work** - Set before spawn, sync automatically
4. **Simpler client code** - No special reconnection handling needed
5. **Testable** - Each piece can be tested independently
6. **Maintainable** - Clear separation of concerns

---

## Questions to Consider

1. **What about the opponent's view?** When player despawns, opponent sees them disappear briefly. Is this acceptable? Could show "Reconnecting..." UI.

2. **Board state?** Cards on board are separate objects. Need to either:
   - Preserve them (don't despawn)
   - Respawn them too
   - Have them owned by server, not player

3. **Turn timing?** If it's the disconnected player's turn, what happens? Probably pause the turn timer.

4. **Multiple reconnects?** What if player disconnects/reconnects multiple times?

---

## Next Steps

1. Decide on approach (recommend Option A)
2. Create `DisconnectedPlayerState` class
3. Implement disconnect state capture
4. Implement reconnect state restoration
5. Remove all existing bandaid code
6. Test thoroughly

Would you like me to proceed with implementing Option A?
