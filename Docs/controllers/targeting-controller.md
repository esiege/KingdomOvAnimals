# TargetingController

Validates and manages target selection for abilities and attacks.

## Responsibilities

- Validate targets based on ability type
- Highlight valid/invalid targets
- Track current target selection state
- Support multiple targeting modes

## Target Types

See [AbilityTargetType](../abilities.md#target-types) for enum values:

| Type | Description |
|------|-------------|
| `SingleEnemy` | One enemy creature |
| `SingleAlly` | One friendly creature |
| `AllEnemies` | All enemy creatures |
| `AllAllies` | All friendly creatures |
| `Self` | The source card only |
| `None` | No target required |

## Validation

### `IsValidTarget(source, target, targetType)`
Checks if target is valid for given ability:
1. Target must exist and be in play
2. Target ownership matches targetType
3. Target not immune/protected

### `GetValidTargets(source, targetType)`
Returns list of all valid targets for ability type.

## Visual Feedback

| State | Visual |
|-------|--------|
| Valid target | Green highlight glow |
| Invalid target | Red highlight glow |
| Selected target | Yellow outline |
| Targeting mode active | Dim non-targets |

## Methods

| Method | Description |
|--------|-------------|
| `StartTargeting(card, ability)` | Enter targeting mode |
| `CancelTargeting()` | Exit without selecting |
| `ConfirmTarget(target)` | Execute ability on target |
| `HighlightValidTargets()` | Show valid targets |
| `ClearHighlights()` | Remove all highlights |

## Attack Targeting

Separate from ability targeting:
- Only valid during your turn
- Card must not have summoning sickness
- Card must not be tapped
- Target must be enemy creature or face

### Taunt Mechanic
If enemy has creature with Taunt, must target that creature first.

## Network Flow

1. Local: Validate target
2. Local: Send `CmdUseAbility` with target ID
3. Server: Re-validate target
4. Server: Execute ability
5. Server: Broadcast result to all clients

---
*Parent: [Controllers](./README.md)*
