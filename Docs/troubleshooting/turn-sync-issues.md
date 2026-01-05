# Turn Synchronization Issues

Issues related to networked turn management and duplicate actions.

---

## Issue: Duplicate Turn Actions (Mana Refill/Card Draw)

**Date Discovered:** January 3, 2026  
**Severity:** Medium  
**Status:** Resolved

### Symptoms
- Both players calling `CmdRefillMana` when turn changes
- Multiple mana refill logs appearing
- Turn start actions executing on wrong client

### Root Cause

`StartTurnNetwork()` was executing turn-start actions regardless of whose turn it was. Both the active player AND the opponent were running turn-start logic.

### Bad Pattern
```
Turn changes → Both clients call StartTurnNetwork()
                 → Both clients refill mana
                 → Both clients draw cards
```

### Fixed Pattern
```
Turn changes → Both clients call StartTurnNetwork()
                 → Only active player refills mana
                 → Only active player draws card
```

### Key Learning

Always check `isLocalPlayerTurn` before executing turn-start actions. The SyncVar callback fires on ALL clients, but actions should only execute on the active player's client.

### Related Files
- `Assets/Scripts/Controllers/EncounterController.cs` - `StartTurnNetwork()`
- `Assets/Scripts/Network/NetworkGameManager.cs` - Turn state sync

---

## Issue: First Turn Special Handling

**Date Discovered:** January 4, 2026  
**Severity:** Low  
**Status:** Resolved

### Symptoms
- First turn player gets extra mana
- First turn causes double card draw (initial hand + turn start draw)

### Root Cause

Turn 1 was treated the same as subsequent turns, but players already:
- Drew initial hands during `OnNetworkGameStarted`
- Have starting mana set

### Resolution

Added special handling in `OnNetworkTurnChanged`:
- Turn 1: Only enable UI interactions (no draw/mana)
- Turn 2+: Full turn-start logic (draw, mana refill, board reset)

### Key Learning

Game initialization and turn-start are different operations. Track whether the game is fully initialized before running turn logic.

---

## Issue: Turn Actions After Game End

**Date Discovered:** January 4, 2026  
**Severity:** Low  
**Status:** Addressed

### Symptoms
- Players could still take actions after opponent forfeited
- End turn continued to work after game over

### Resolution

Added `isGamePaused` flag to EncounterController:
- Set true on disconnect
- Blocks turn-end and card play actions
- Cleared on reconnection

---
*Parent: [Troubleshooting](./README.md)*
