# Story 023: Ability Execution System

## Status: Not Started
## Sprint: 03
## Dependencies: 018, 022
## Started: 

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

- [ ] AbilityExecutor.Execute(AbilityData, caster, target) runs ability
- [ ] Routes to correct behavior based on behaviorType
- [ ] DamageAbility deals damage to target
- [ ] HealAbility heals target
- [ ] PoisonAbility applies damage-over-time
- [ ] StunAbility disables card for N turns
- [ ] BuffAttackAbility increases attack stat
- [ ] ReturnToHandAbility returns card to hand
- [ ] DrawCardAbility draws card from deck
- [ ] Network synced (all clients see effect)

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
