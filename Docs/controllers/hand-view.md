# HandView

`Assets/Scripts/View/HandView.cs` — lays out one player's hand (one `HandView` per player, `PlayerId` set in
the inspector/setup). Re-renders on `NetworkBoardState.OnStateChanged`.

## Responsibilities

- Keeps a pooled list of `CardView`s in sync with `BoardState.Players[PlayerId].Hand.Count` (creates/destroys
  as needed rather than instantiating fresh every render)
- Positions cards in a fan: `_cardSpacing`, `_cardFanAngle`, `_cardArcHeight` control spread/rotation/arc
- Calls `card.SetFaceDown(true)` for every card when `IsLocalHand == false` (opponent's hand)
- `OnCardHover` / `OnCardUnhover` lift the hovered card and bring it to front (local hand only)

## Key Members

| Member | Description |
|--------|-------------|
| `PlayerId` | Which player's hand this instance renders |
| `IsLocalHand` | If false, all cards render face-down |
| `RenderHand(state)` | Full re-render from `BoardState` |
| `GetCardAtIndex(index)` / `GetAllCards()` | Used by `InputController` for hit-testing |
| `Clear()` | Destroy all card views |

---
*Parent: [Controllers/View](./README.md)*
