# InputController

`Assets/Scripts/View/InputController.cs` — translates mouse input into `NetworkBoardState.Cmd*` calls. Ignores
all input when it isn't the local player's turn (`NetworkBoardState.IsPlayerTurn(LocalPlayerId)`).

## Input Scheme

**Left-click** is the offensive/play action:

| You click/drag | Drop target | Result |
|-----------------|-------------|--------|
| Hand card | Empty own slot | `CmdPlayCard` |
| Hand card | Enemy board card | `CmdUseFlipAbility` (offensive ability, from hand) |
| Hand card | Opponent avatar | `CmdUseFlipAbilityOnPlayer` |
| Board card (can act) | Enemy board card | `CmdUseBoardAbility` |
| Board card (can act) | Opponent avatar | `CmdAttackPlayer` |

**Right-click** on your own actionable board card enters support-targeting mode; left-clicking a friendly board
card then fires `CmdUseSupportAbility`. **There is no path to use a support ability from a hand card** — despite
[game-design/targeting.md](../game-design/targeting.md) describing "drag to friendly unit" from hand, that flow
isn't wired up in `InputController`, and `NetworkBoardState.CmdUseSupportAbility` requires a board-card source.

## State Machine

`Idle → DraggingHandCard / DraggingBoardCard → (drop) → Idle`, plus a separate `SelectingSupportTarget` state
entered by right-click. `ClearSelection()` resets state and clears highlights on release/cancel.

## Target Detection

`GetCardUnderMouse()` tries a `Physics2D` raycast against `_cardLayerMask` first, then falls back to manual
`RectTransform` hit-testing over `HandView`/`BoardView`'s tracked cards (for UI-based cards without colliders).
`IsOverOpponentAvatar()` checks `_playerAvatarLayerMask` or, as a fallback, `BoardView.OpponentAvatarZone`
directly.

---
*Parent: [Controllers/View](./README.md)*
