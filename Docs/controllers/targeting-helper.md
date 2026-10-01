# TargetingHelper

`Assets/Scripts/Logic/TargetingHelper.cs` (`KOA.Logic`) — pure, stateless target-list calculation. No Unity
state, so it's directly unit-testable.

## Methods

| Method | Description |
|--------|-------------|
| `GetValidBoardAbilityTargets(state, attackerPlayerId, attackerSlotIndex, cardLibrary)` | Targets for a board card's ability, using its **offensive** ability's `TargetType` (defaults to `SingleEnemy` if none) |
| `GetValidFlipAbilityTargets(state, attackerPlayerId, handIndex, cardLibrary)` | Targets for a hand card's flip ability, using its **defensive** ability's `TargetType` (defaults to `SingleEnemy` if none) — note this reads the *defensive* ability's target type even though the flip attack itself deals *offensive* damage server-side |
| `GetValidPlaySlots(state, playerId)` | Empty board slots available to play into |
| `IsValidTarget(validTargets, playerId, slotIndex)` | Membership check |
| `GetMaxHealth(card, library)` | Max health from the card's `CardData` |

## TargetType handling (`GetTargetsForType`)

| `TargetType` | Targets returned |
|---------------|-------------------|
| `SingleEnemy` / `AllEnemies` | All occupied enemy slots |
| `SingleAlly` / `AllAllies` | All occupied friendly slots |
| `BoardWide` | All occupied slots, both sides |
| `Self` | None yet — needs source-slot context not currently passed in |
| `EnemyPlayer` | None — player-direct targeting is handled separately by `InputController`'s avatar-zone detection, not through this helper |

`SingleEnemy`/`AllEnemies` and `SingleAlly`/`AllAllies` currently return identical results — there's no
single-vs-all distinction implemented yet at this layer.

---
*Parent: [Controllers/View](./README.md)*
