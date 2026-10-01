# Card System

*Verified against source: 2026-08-12 (vdate)*

A card is described by two objects: a `CardData` template (the ScriptableObject asset — art, base stats,
abilities) and a `CardState` instance (the live, per-copy runtime state). See
[architecture.md](../architecture.md#model) for the full picture.

## CardData (template — `Assets/Scripts/Data/CardData.cs`)

| Property | Description |
|----------|--------------|
| `id` | Unique string ID, used as the network-safe reference and `CardLibrary` lookup key |
| `displayName` | Shown in `CardView` |
| `health` | Base/max health |
| `manaCost` | Mana required to play |
| `artwork` | Card art sprite |
| `offensiveAbility` | `AbilityData` used when this card attacks (hand flip or board attack) |
| `defensiveAbility` | `AbilityData` used for its support action |

## CardState (runtime instance — `Assets/Scripts/Model/CardState.cs`)

| Field | Description |
|-------|--------------|
| `InstanceId` | Unique per-copy ID, links `CardState` to its `CardView` |
| `CardDataId` | Looks up the `CardData` template via `CardLibrary` |
| `OwnerId` | 0 or 1 |
| `CurrentHealth` | May differ from `CardData.health` due to damage/healing |
| `IsTapped` | Used its action this turn |
| `HasSummoningSickness` | Just played, can't act until next turn |
| `IsFrozen` | Can't act (status effect) |
| `IsDefending` | Defensive stance flag |
| `IsFlipped` | Face-down flag |

`CanAct` = valid && not tapped && no sickness && not frozen. Whether a card is "in hand" or "on board" is not a
flag on `CardState` itself — it's positional: a card is wherever it appears in `PlayerBoardState.Hand` or
`PlayerBoardState.Board[]`.

## Rendering (`CardView`)

One prefab renders both hand and board cards — no separate "Full" vs. "Condensed" prefabs. See
[card-view.md](../controllers/card-view.md).

## Lifecycle

```
1. Card ID in Deck (List<string> in PlayerBoardState.Deck)
       │  DrawCard()
       ▼
2. CardState created, added to Hand
       │  CmdPlayCard
       ▼
3. Placed on Board[slotIndex], HasSummoningSickness = true, mana spent
       │  next turn: PlayerBoardState.OnTurnStart()
       ▼
4. HasSummoningSickness = false — can act
       │  Cmd(Board/Flip)Ability
       ▼
5. IsTapped = true — reset to false at the next OnTurnStart()
       │  CurrentHealth <= 0
       ▼
6. Removed from Board[] via PlayerBoardState.RemoveCard(); CardView plays death animation and destroys
```

There is currently no graveyard list in `PlayerBoardState` — dead cards are simply removed, not tracked.

---
*Parent: [Game Design](./README.md)*
