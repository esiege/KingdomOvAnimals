# BoardView

`Assets/Scripts/View/BoardView.cs` — renders `BoardState` onto scene transforms. No game rules.

## Responsibilities

- Owns visual perspective via `LocalPlayerId` — `GetSlotTransform(playerId, slotIndex)` mirrors the opponent's
  front/back slots (0↔2) so both players see their own cards as "near side"
- Creates/updates/destroys `CardView` instances to match `BoardState` (`RenderBoard()`)
- Tracks all live `CardView`s in a `Dictionary<int, CardView>` keyed by `CardState.InstanceId`
- Highlights valid target slots for `InputController`

## Setup (`Start()`)

Finds the local `NetworkPlayer` (falls back to `NetworkGameManager.GetLocalPlayer()`) to set `LocalPlayerId`,
then subscribes to `NetworkBoardState.OnStateChanged` / `OnCardPlayed` / `OnCardDamaged` / `OnCardDied` and does
an initial `RenderBoard()`.

## Key Members

| Member | Description |
|--------|-------------|
| `LocalPlayerId` | 0 or 1 — determines which slot array is "my side" |
| `GetSlotTransform(playerId, slotIndex)` | Visual slot transform, perspective-corrected |
| `GetHandArea(playerId)` | Hand container transform |
| `CreateCardView(state)` / `GetCardView(id)` / `DestroyCardView(id)` | CardView lifecycle |
| `RenderBoard(state)` | Full re-render: create/update/destroy views to match state |
| `HighlightSlots(slots, bool)` / `ClearAllHighlights()` | Targeting feedback |
| `OpponentAvatarZone` | Collider used by `InputController` to detect "attack player" drops |

Scene wiring (`_player0Slots`, `_player1Slots`, `_myHandArea`, `_opponentHandArea`, avatar zone colliders,
`_cardViewPrefab`, `_cardLibrary`) is validated by **KOA → Board State Setup**.

---
*Parent: [Controllers/View](./README.md)*
