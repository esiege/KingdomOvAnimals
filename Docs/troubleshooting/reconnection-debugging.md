# Reconnection Debugging

Issues related to reconnection system debugging and object lifecycle.

---

## Issue: Reconnection Logs Stop After Host Disconnect

**Date Discovered:** January 5, 2026  
**Severity:** High  
**Status:** Under Investigation

### Symptoms
- `log_editor.txt` shows "Log ended" immediately after host disconnect
- Reconnection attempt logs never appear
- `_isWaitingForHostReconnect` set to true but Update() loop messages don't log
- About 1 second gap between disconnect and log end

### Investigation

1. **ConsoleLogToFile destroyed**: The `OnDestroy()` callback fires ~1 second after disconnect
2. **PlayerConnectionHandler destroyed**: Also destroyed at same time despite `DontDestroyOnLoad`
3. **Both objects use singleton pattern**: Both have `DontDestroyOnLoad(gameObject)` in Awake

### Root Cause (Suspected)

The objects are being destroyed despite `DontDestroyOnLoad`. Possible causes:
1. Scene reload triggered by FishNet on disconnect
2. Duplicate instances in DuelScreen scene causing destruction cascade
3. NetworkManager cleanup destroying scene objects

### Diagnostic Logging Added

Separate reconnect log file (`reconnect_editor.txt`) using direct file I/O:
- Independent of Unity's logging system
- Writes to file with `AutoFlush = true`
- Logs instance IDs and scene names in Awake
- Logs destruction state in OnDestroy

### Key Log Output Pattern

```
Awake called on instance {id}, scene={sceneName}
Instance {id} set as singleton
DontDestroyOnLoad called for instance {id}
...
OnDestroy called on instance {id}! _isWaitingForHostReconnect=True
WARNING: Singleton instance {id} is being destroyed!
```

### Debugging Steps

1. Check if duplicate instances exist in both MainMenu and DuelScreen scenes
2. Look for scene reload in FishNet disconnect handling
3. Verify DontDestroyOnLoad actually moves object to persistent scene
4. Check FishNet's ClientObjects.OnClientConnectionState behavior

### Workarounds Attempted

- Added instance ID logging to track which object is destroyed
- Created separate file-based logging independent of ConsoleLogToFile
- Added `Instance == this` check in OnDestroy

### Related Files
- `Assets/Scripts/Network/PlayerConnectionHandler.cs`
- `Assets/Scripts/Utilities/ConsoleLogToFile.cs`

---

## Issue: Reconnection Timer Never Fires

**Date Discovered:** January 5, 2026  
**Severity:** High  
**Status:** Blocked by Object Destruction Issue

### Symptoms
- `AttemptReconnect()` should fire every 3 seconds
- "Reconnect timer expired" log message never appears
- Update() loop appears to stop running

### Root Cause

The PlayerConnectionHandler GameObject is destroyed before the timer fires. See "Reconnection Logs Stop After Host Disconnect" above.

### Resolution

Must fix object destruction issue first. Timer logic is correct but object doesn't survive to execute it.

---
*Parent: [Troubleshooting](./README.md)*
