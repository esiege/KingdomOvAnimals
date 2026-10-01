# Story 023: Ability Execution System

## Status: Done (superseded)
## Sprint: 03
## Dependencies: 018, 022
## Started: 

> **Reclassified 2026-08-12 (vdate):** self-reported "Not Started" was stale. The goal (abilities execute with
> correct effects) shipped, but via Story 036's `NetworkBoardState` rewrite rather than this story's originally
> planned `AbilityExecutor` class — `CmdUseBoardAbility`/`CmdUseFlipAbility`/`CmdUseSupportAbility` in
> `NetworkBoardState.cs` do this today. Not every AC below (PoisonAbility DoT, StunAbility) is necessarily wired
> through those commands yet — verify against `Assets/Scripts/Abilities/Behaviors/` before assuming full parity.

---

## User Story

**As a** player  
**I want** abilities to execute with correct effects  
**So that** combat works as designed

---

## Description

- Create AbilityExecutor to run abilities
- Use AbilityBehavior system (DamageAbility, HealAbility, etc.)
- Execute with AbilityContext (caster, target, abilityData)
- Apply effects to targets
- Network sync ability execution

---

## Acceptance Criteria

- [ ] DamageAbility reduces target's health by the correct amount
- [ ] HealAbility increases target's health by the correct amount
- [ ] PoisonAbility applies damage over multiple turns
- [ ] StunAbility prevents target from acting
- [ ] BuffAttackAbility increases target's attack stat
- [ ] ReturnToHandAbility sends card back to hand
- [ ] DrawCardAbility adds a card to hand from deck
- [ ] In multiplayer, both players see ability effects

---

## Technical Notes

### AbilityExecutor.cs
```csharp
public static class AbilityExecutor
{
    public static void Execute(AbilityData ability, CardController caster, CardController target)
    {
        if (ability == null) return;
        
        var behavior = AbilityBehaviorRegistry.GetBehavior(ability.behaviorType);
        if (behavior == null)
        {
            Debug.LogError($"No behavior for: {ability.behaviorType}");
            return;
        }
        
        var context = new AbilityContext
        {
            caster = caster,
            target = target,
            abilityData = ability
        };
        
        behavior.Execute(context);
    }
}
```

### Network Sync
```csharp
[ServerRpc]
public void UseAbilityServerRpc(string abilityId, NetworkObjectReference targetRef)
{
    // Validate, execute, then sync to clients
    ExecuteAbilityClientRpc(abilityId, targetRef);
}

[ClientRpc]
private void ExecuteAbilityClientRpc(string abilityId, NetworkObjectReference targetRef)
{
    // Visual effects, sound, animation
}
```

---

## Definition of Done

- [ ] Code complete
- [ ] Unit tests pass (if applicable)
- [ ] Acceptance criteria met
- [ ] Code reviewed
- [ ] Merged to main branch
- [ ] Story closed in backlog

---

## Notes

- Use existing AbilityBehavior classes from Story 018
- Ensure network authority (only server executes)
- Clients receive results for visuals/audio
- Test all 7 behavior types
