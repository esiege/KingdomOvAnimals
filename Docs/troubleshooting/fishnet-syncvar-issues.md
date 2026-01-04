# FishNet SyncVar Troubleshooting

## Issue 001: SyncVar Default Values Not Syncing to Clients

**Date Discovered:** January 3, 2026  
**Severity:** High  
**Status:** Resolved

### Symptoms
- `[NetworkPlayer] Player 1 health: 20 -> 0` appearing in logs
- Only affects non-owner clients viewing other players' NetworkPlayer objects
- Server shows correct values, but clients receive 0 (default int value)

### Root Cause
FishNet's SyncVar constructor defaults (e.g., `new SyncVar<int>(20)`) set the value locally but **do not mark it as "dirty"** for network synchronization. When serializing initial state to clients, FishNet only includes values that have been explicitly "set" after object creation.

Since the constructor default was never "changed", FishNet treats it as unchanged and doesn't serialize it - clients receive `0` (the C# default for int).

### Bad Code
```csharp
// DON'T DO THIS - constructor defaults won't sync properly
public readonly SyncVar<int> CurrentHealth = new SyncVar<int>(20);
public readonly SyncVar<int> MaxHealth = new SyncVar<int>(20);
public readonly SyncVar<int> CurrentMana = new SyncVar<int>(1);
public readonly SyncVar<int> MaxMana = new SyncVar<int>(1);

public override void OnStartServer()
{
    base.OnStartServer();
    // Values are already 20/1 from constructor, so they're not "dirty"
    // and won't be serialized to clients!
}
```

### Fixed Code
```csharp
// DO THIS - no constructor defaults
public readonly SyncVar<int> CurrentHealth = new SyncVar<int>();
public readonly SyncVar<int> MaxHealth = new SyncVar<int>();
public readonly SyncVar<int> CurrentMana = new SyncVar<int>();
public readonly SyncVar<int> MaxMana = new SyncVar<int>();

public override void OnStartServer()
{
    base.OnStartServer();
    
    // MUST explicitly set values here - this marks them as dirty
    // so they get serialized to clients
    CurrentHealth.Value = 20;
    MaxHealth.Value = 20;
    CurrentMana.Value = 1;
    MaxMana.Value = 1;
}
```

### Key Learnings
1. **Never rely on SyncVar constructor defaults** for values that need to sync
2. **Always set SyncVar values in `OnStartServer()`** to ensure they're marked dirty
3. The `OnStartServer()` callback runs after spawn, so values set here will be included in the initial serialization to clients
4. This issue only affects the **initial sync** - subsequent `.Value` assignments work correctly

### Related Files
- `Assets/Scripts/Network/NetworkPlayer.cs`

---

## Issue 002: Duplicate Turn Actions (Mana Refill/Card Draw)

**Date Discovered:** January 3, 2026  
**Severity:** Medium  
**Status:** Resolved

### Symptoms
- Both players calling `CmdRefillMana` when turn changes
- Multiple mana refill logs appearing
- Turn start actions executing on opponent's client

### Root Cause
The `StartTurnNetwork()` method was performing turn-start actions (mana refill, card draw, ResetBoard) regardless of whether it was the current player's turn or the opponent's turn.

### Bad Code
```csharp
private void StartTurnNetwork()
{
    if (isCurrentPlayerTurn)
    {
        // Do turn start stuff...
    }
    else
    {
        // This ALSO did turn start stuff for the opponent!
        currentPlayer = opponentController;
        CmdRefillMana(); // WRONG - opponent shouldn't refill your mana!
        DrawCard(currentPlayer);
    }
}
```

### Fixed Code
```csharp
private void StartTurnNetwork()
{
    if (isCurrentPlayerTurn)
    {
        // Your turn - do all turn start actions
        currentPlayer = playerController;
        CmdRefillMana();
        DrawCard(currentPlayer);
        ResetBoard();
        // Enable UI...
    }
    else
    {
        // Opponent's turn - just update state, no actions
        currentPlayer = opponentController;
        // Disable UI, show waiting indicator...
    }
}
```

### Key Learnings
1. Turn-start actions should **only execute on the active player's client**
2. The opponent's client should only update local state (currentPlayer) and UI
3. Server-authoritative values (mana) are synced via SyncVars, so opponents don't need to trigger their own refills

### Related Files
- `Assets/Scripts/Controllers/EncounterController.cs`
