# Story 006: Sync Combat and Abilities

## Story Information

- **Story ID**: 006
- **Priority**: High
- **Sprint**: Sprint 01
- **Status**: Done
- **Created**: 2026-01-04
- **Started**: 2026-01-04
- **Completed**: 2026-01-04

## User Story

**As a** player  
**I want** to see combat actions and ability effects happen on both screens  
**So that** both players see the same game state after attacks and abilities

## Background / Context

Currently, combat and abilities are handled locally:
- Player clicks a card to attack
- Damage is calculated and applied locally
- Cards die/are removed locally
- Opponent's client doesn't know about these changes

For multiplayer, when Player A attacks:
- Attack action sent to server
- Server validates the attack (tapped state, valid target)
- Server calculates damage
- Server broadcasts result to all clients
- Both clients update health, remove dead cards

## Acceptance Criteria

### AC1: Attack action syncs to opponent
**Test:** Player attacks opponent's card
**Expected:** Opponent sees the attack animation/effect

### AC2: Damage syncs to both players
**Test:** Player deals 3 damage to opponent's 5-health card
**Expected:** Both players see card health go from 5 to 2

### AC3: Card death syncs
**Test:** Player kills opponent's card
**Expected:** Card is removed from board on both screens

### AC4: Ability effects sync
**Test:** Player uses a heal ability
**Expected:** Both players see the heal effect applied

### AC5: Tapped state syncs
**Test:** Player taps a card to attack
**Expected:** Opponent sees the card become tapped

---

## Tasks

- [x] Create ServerRpc for attack actions
- [x] Server validates attack (card can attack, valid target)
- [x] Server calculates and applies damage
- [x] Broadcast damage/death to all clients via ObserversRpc
- [x] Sync tapped state when cards attack
- [x] Sync ability usage and effects

---

## Technical Notes

### Key Files
- `Assets/Scripts/Abilities/DamageAbility.cs` - Current damage logic
- `Assets/Scripts/Controllers/CardController.cs` - Card health/death
- `Assets/Scripts/Network/NetworkPlayer.cs` - Add combat RPCs here

### Approach
1. Move damage calculation to server
2. Add `CmdAttack(targetCardId)` ServerRpc
3. Add `RpcApplyDamage(cardId, damage)` ObserversRpc
4. Add `RpcRemoveCard(cardId)` for death sync
