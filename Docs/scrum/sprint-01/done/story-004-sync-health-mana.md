# Story 004: Sync Player Health and Mana Across Network

## Story Information

- **Story ID**: 004
- **Priority**: High
- **Sprint**: Sprint 01
- **Status**: ✅ Complete
- **Created**: 2026-01-03
- **Started**: 2026-01-03
- **Completed**: 2026-01-03

## User Story

**As a** player  
**I want** to see my opponent's health and mana in real-time  
**So that** I can make strategic decisions based on the game state

## Background / Context

The existing `PlayerController` manages local health/mana state with UI updates. For multiplayer, we need to sync these values across the network so both players see accurate game state.

Current PlayerController has:
- `currentHealth`, `maxHealth` - Player health
- `currentMana`, `maxMana` - Player mana resource
- Methods: `TakeDamage()`, `Heal()`, `SpendMana()`, `RefillMana()`, etc.

We need to:
1. Make PlayerController network-aware (extend NetworkBehaviour)
2. Use SyncVar for health/mana values
3. Ensure only server authorizes changes
4. Update UI on all clients when values change

## Acceptance Criteria

### AC1: Health syncs on damage
**Test:** Player 1 takes 5 damage
**Expected:** Both Player 1 and Player 2's screens show Player 1's updated health

### AC2: Mana syncs on spend
**Test:** Player 1 spends 3 mana to play a card
**Expected:** Both players see Player 1's mana decrease by 3

### AC3: Server authority on health changes
**Test:** Client attempts to modify health directly
**Expected:** Change is rejected or routed through server RPC

### AC4: UI updates on sync
**Test:** Any health/mana change from network
**Expected:** UI text elements update immediately on all clients

---

## Tasks

- [ ] Create NetworkPlayerState.cs (NetworkBehaviour for synced player data)
- [ ] Add SyncVar<int> for health and mana values
- [ ] Create ServerRpc methods for TakeDamage, Heal, SpendMana, etc.
- [ ] Add OnChange callbacks to update PlayerController UI
- [ ] Link NetworkPlayerState to existing PlayerController
- [ ] Test: 2 players - verify health sync
- [ ] Test: 2 players - verify mana sync

---

## Technical Notes

### Architecture
We consolidated health/mana syncing into the existing `NetworkPlayer` class (which is spawned per-connection) rather than using a separate `NetworkPlayerState` component.

**Why?**
- `NetworkPlayer` is already spawned for each connection
- Keeps all player network state in one place
- `NetworkGameManager` links spawned `NetworkPlayer` to scene `PlayerController`

### Flow
1. Players connect in MainMenu → NetworkPlayer spawned per connection
2. Scene loads to DuelScreen
3. `NetworkGameManager.OnStartClient()` finds all NetworkPlayers
4. Links them to local PlayerController/OpponentController
5. SyncVar callbacks update the linked PlayerController + UI

### FishNet v4 SyncVar Pattern
```csharp
public readonly SyncVar<int> CurrentHealth = new SyncVar<int>(20);

private void Awake()
{
    CurrentHealth.OnChange += OnHealthChanged;
}

private void OnHealthChanged(int prev, int next, bool asServer)
{
    // Update linked PlayerController
    if (LinkedPlayerController != null)
    {
        LinkedPlayerController.currentHealth = next;
        LinkedPlayerController.UpdatePlayerUI();
    }
}

[ServerRpc(RequireOwnership = false)]
public void CmdTakeDamage(int amount)
{
    CurrentHealth.Value -= amount;
}
```

---

## Files Created/Modified

| File | Action | Purpose |
|------|--------|---------|
| `Assets/Scripts/Network/NetworkPlayer.cs` | Modified | Added SyncVar for health/mana, ServerRpc methods |
| `Assets/Scripts/Network/NetworkGameManager.cs` | Created | Links NetworkPlayer to scene PlayerControllers |
| `Assets/Scripts/Controllers/PlayerController.cs` | Modified | Routes health/mana through NetworkPlayer |
| `Assets/Scripts/Editor/DuelSceneNetworkSetup.cs` | Created | Editor tool to set up DuelScreen |

---

## Setup Instructions

1. Open DuelScreen scene
2. Go to **Tools > KingdomOvAnimals > Setup DuelScreen Networking**
3. Click "Setup DuelScreen for Networking"
4. Verify PlayerController assignments in NetworkGameManager Inspector
5. Delete NetworkHudCanvas if present
6. Save the scene

---

## Definition of Done

- [ ] Both players see each other's health in real-time
- [ ] Both players see each other's mana in real-time
- [ ] All health/mana changes go through server
- [ ] UI updates automatically on sync
- [ ] No compile errors
- [ ] Tested with 2 connected players
