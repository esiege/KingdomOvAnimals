# EndTurnController

UI component handling the end turn button and turn indicators.

## Responsibilities

- Show/hide end turn button based on turn ownership
- Display current turn indicator
- Handle end turn button clicks
- Show waiting state when not your turn

## References

| Reference | Description |
|-----------|-------------|
| `encounterController` | To call EndTurn() |
| `endTurnButton` | Button UI element |
| `turnIndicatorText` | "Your Turn" / "Opponent's Turn" |
| `waitingPanel` | Shown during opponent's turn |

## State Display

| State | Button | Text |
|-------|--------|------|
| Your turn | Enabled, visible | "Your Turn" |
| Opponent turn | Disabled/hidden | "Waiting..." |
| Game paused | Disabled | "Paused" |

## Methods

| Method | Description |
|--------|-------------|
| `OnTurnChanged(isYourTurn)` | Update UI for turn change |
| `OnEndTurnClicked()` | Button handler, calls EndTurn |
| `SetInteractable(bool)` | Enable/disable button |

## Network Integration

- Button disabled during network operations
- Re-enabled after server confirms turn end
- Shows spinner during RPC processing

---
*Parent: [Controllers](./README.md)*
