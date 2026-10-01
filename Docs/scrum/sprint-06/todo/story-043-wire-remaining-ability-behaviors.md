# Story 043: Wire Remaining Ability Behaviors Into NetworkBoardState

## Status: Not Started
## Sprint: 06
## Dependencies: None
## Created: 2026-08-12

## Background

`AbilityBehaviorRegistry` auto-discovers `DamageAbility`, `HealAbility`, `BuffAttackAbility`, `PoisonAbility`,
`StunAbility`, `DrawCardAbility`, `ReturnToHandAbility` from `Assets/Scripts/Abilities/Behaviors/`, but
`NetworkBoardState`'s `Cmd*` methods only clearly apply instant damage/heal today (`ExecuteSupportAbility` only
branches on "heal" and "buff" behavior types, with buff logged as TODO). Duration-based effects (Poison DoT,
Stun) don't have an obvious tick/expiry mechanism in `CardState`/`BoardState` yet.

## User Story
**As a** player
**I want** every implemented ability behavior to actually apply its effect in a real match
**So that** cards with Poison/Stun/Buff abilities aren't silently no-ops

## Acceptance Criteria
- [ ] `NetworkBoardState.ExecuteSupportAbility` (or its replacement) routes through `AbilityBehaviorRegistry`
      instead of hand-checking `behaviorType.Contains(...)`
- [ ] Duration-based status effects (Poison, Stun) have a place to live on `CardState` and tick down at
      `OnTurnStart()`
- [ ] `BuffAttackAbility` actually modifies the target's effective offensive damage, not just a log line
- [ ] Each behavior has a manual test showing the effect applied and visible in `CardView`

## Notes
Touches `Assets/Scripts/Network/NetworkBoardState.cs`, `Assets/Scripts/Model/CardState.cs`,
`Assets/Scripts/Abilities/`.
