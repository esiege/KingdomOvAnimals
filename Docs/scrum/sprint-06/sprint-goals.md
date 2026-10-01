# Sprint 06 Goals

*(planned 2026-08-12, not yet started)*

## Primary Goal

**Finish the core duel loop's remaining gaps — deck selection and full ability behavior coverage — so Adventure
Mode has a complete, trustworthy combat loop to build on.**

## Success Criteria

By the end of this sprint, we should have:

1. [ ] Players can select a deck before a match instead of both auto-loading the same default deck (Story 024)
2. [ ] Every `AbilityBehaviorRegistry` behavior (Poison, Stun, BuffAttack, etc.) actually applies its effect
      through `NetworkBoardState`, not just instant damage/heal (Story 043)

## Key Focus Areas

### 1. Deck Selection
Today `NetworkGameManager.ServerStartGame` loads one default deck for both players. Real deck choice is a
prerequisite for Adventure Mode's "assemble your deck through a run" premise anyway.

### 2. Ability Behavior Completeness
Duration-based effects (Poison DoT, Stun) don't have an obvious tick/expiry mechanism on `CardState` yet, and
`ExecuteSupportAbility` still hand-checks behavior-type strings instead of routing through the registry.

## Non-Goals (Out of Scope)

- New ability types beyond what's already in `Abilities/Behaviors/`
- Adventure Mode work (starts Sprint 07)
- Deck-building UX beyond selection (Adventure Mode's deck-mixing UX is Sprint 14)

## Definition of Done for Sprint

- [ ] Deck selection UI functional, server validates the choice
- [ ] Every implemented ability behavior demonstrably applies its effect in a manual test, visible in `CardView`
