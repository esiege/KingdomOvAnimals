# DontDestroyOnLoad Issues

Issues related to Unity's persistence system and object lifecycle.

---

## Issue: DontDestroyOnLoad Objects Still Destroyed

**Date Discovered:** January 5, 2026  
**Severity:** High  
**Status:** Under Investigation

### Symptoms
- Objects with `DontDestroyOnLoad()` in Awake are destroyed
- Destruction occurs ~1 second after network disconnect
- OnDestroy callback fires despite persistence flag

### Affected Components
- `PlayerConnectionHandler`
- `ConsoleLogToFile`

### Expected Behavior

`DontDestroyOnLoad(gameObject)` should:
1. Move object to special "DontDestroyOnLoad" scene
2. Prevent destruction on scene unload
3. Only destroy via explicit `Destroy()` call or application quit

### Actual Behavior

Objects are destroyed shortly after host disconnect event, even though:
- `DontDestroyOnLoad()` was called in Awake
- No explicit `Destroy()` call in code path
- Application is still running

### Possible Causes

1. **Duplicate Instances**: Object exists in both MainMenu and DuelScreen scenes
   - First instance calls DontDestroyOnLoad
   - Second instance's Awake destroys "duplicate" but wrong one is identified
   
2. **FishNet Scene Management**: FishNet may force scene reload on disconnect
   - Could recreate scene objects
   - Might destroy existing DontDestroyOnLoad objects
   
3. **Parent Object Destruction**: If attached to child of NetworkManager
   - NetworkManager destruction cascades to children
   
4. **Singleton Pattern Race**: Multiple instances in flight during scene load

### Investigation Checklist

- [ ] Verify object is in DontDestroyOnLoad scene after Awake
- [ ] Check for duplicate GameObjects in DuelScreen scene
- [ ] Log parent hierarchy in Awake
- [ ] Check if FishNet triggers scene reload on client disconnect
- [ ] Verify singleton Instance reference points to correct object

### Diagnostic Code

Added to Awake:
```csharp
LogReconnect($"Awake called on instance {GetInstanceID()}, scene={gameObject.scene.name}");
```

Added to OnDestroy:
```csharp
LogReconnect($"OnDestroy called on instance {GetInstanceID()}!");
if (Instance == this)
    LogReconnect($"WARNING: Singleton instance is being destroyed!");
```

### Potential Fixes

1. **Remove from DuelScreen scene**: Only have object in MainMenu
2. **Check scene before DontDestroyOnLoad**: Skip if already in persistent scene
3. **Use different persistence pattern**: ScriptableObject or static class
4. **Protect against destruction**: Override OnDestroy to prevent (risky)

---

## Issue: Singleton Reset on Scene Load

**Date Discovered:** January 5, 2026  
**Severity:** Medium  
**Status:** Documented

### Symptoms
- `Instance` static reference becomes null
- FindObjectOfType returns wrong instance

### Root Cause

When scene loads with a duplicate component:
1. New instance Awake runs
2. Checks `Instance != null` 
3. Destroys self
4. But static `Instance` might point to old destroyed object

### Resolution

Ensure `Instance = null` in OnDestroy when the singleton itself is destroyed:
```csharp
private void OnDestroy()
{
    if (Instance == this)
        Instance = null;
}
```

---
*Parent: [Troubleshooting](./README.md)*
