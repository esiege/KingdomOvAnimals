# HandController

Manages cards in a player's hand. Handles UI layout, interactions, and ability targeting.

## Responsibilities

- Display cards in hand with proper spacing
- Handle drag-and-drop to board
- Manage ability targeting (line renderer)
- Coordinate with deck panel (visual deck count)

## References

| Reference | Description |
|-----------|-------------|
| `playerController` | Owning player |
| `encounterController` | Game coordinator |
| `targetingController` | Handles target validation |
| `deckCountText` | UI showing remaining deck size |

## Card Display

| Property | Description |
|----------|-------------|
| `handTransform` | Parent transform for hand cards |
| `cardSpacing` | Horizontal gap between cards |
| `maxHandWidth` | Cards compress when hand is full |

### Layout Algorithm
Cards centered horizontally under `handTransform`. Spacing reduces when cards exceed max width.

## Drag-and-Drop

### Start Drag
1. Store original position
2. Bring card to front (sorting)
3. Show valid drop zones

### During Drag
1. Follow mouse/touch position
2. Update hover feedback
3. Ability targeting: draw line to potential target

### End Drag
1. Check if over valid slot
2. If valid: play card via PlayerController
3. If invalid: return to hand position
4. Clear all highlights

## Ability Targeting

When card has ability requiring target:
- `lineRenderer` draws from card to mouse
- Valid targets highlighted green
- Invalid targets highlighted red
- Release over valid target activates ability

### LineRenderer Setup
- `lineWidth` - Line thickness
- `lineColor` - Default line color
- `invalidLineColor` - When over invalid target

## Methods

| Method | Description |
|--------|-------------|
| `AddCard(CardController)` | Add card to hand, reposition all |
| `RemoveCard(CardController)` | Remove from hand, reposition |
| `RefreshHandLayout()` | Recalculate all positions |
| `UpdatePlayableCards()` | Highlight affordable cards |
| `ClearTargetingLine()` | Hide ability line |

## Network Considerations

- Hand contents are local (not synced)
- Card plays validated on server
- If server rejects, card returns to hand
- Opponent hand shows card backs only

---
*Parent: [Controllers](./README.md)*
