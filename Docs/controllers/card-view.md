# CardView

`Assets/Scripts/View/CardView.cs` — visual representation of a single card. One prefab, used for both hand and
board rendering. No game logic — renders `CardState` + `CardData`.

## Data Binding

| UI Field | Source |
|----------|--------|
| `_nameText` | `CardData.displayName` |
| `_manaText` | `CardData.manaCost` |
| `_cardImage` | `CardData.artwork` |
| `_attackText` | `CardData.offensiveAbility.damage` (0 if none) |
| `_defenseText` | `CardData.defensiveAbility.damage` (0 if none) |
| `_healthText` | `CardState.CurrentHealth` (tinted red if below max) |
| `_tappedOverlay` | `CardState.IsTapped` |
| `_summoningSicknessIcon` | `CardState.HasSummoningSickness` |

## Key Members

| Member | Description |
|--------|-------------|
| `Initialize(state, cardLibrary)` | Bind to a `CardState`, look up `CardData`, populate all fields |
| `UpdateFromState(state)` | Refresh dynamic fields (health, tapped, sickness) without re-binding static data |
| `SetBoardPosition(playerId, slotIndex)` / `SetHandPosition(handIndex)` | Track where this card currently is (`BoardPosition`, `IsInHand`, `HandIndex`) |
| `SetHighlight(bool)` | Targeting/selection glow |
| `SetFaceDown(bool)` | Hides content — used for the opponent's hand |
| `InstanceId` / `CardDataId` / `OwnerId` | Mirror of the bound `CardState`'s identity fields |

`PlayDamageAnimation` / `PlayDeathAnimation` are stubs (logged, not yet implemented visually).

---
*Parent: [Controllers/View](./README.md)*
