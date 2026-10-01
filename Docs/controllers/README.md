# Controllers / View / Input

*Verified against source: 2026-08-12 (vdate)*

> This folder used to document `EncounterController`, `PlayerController`, `HandController`,
> `TargetingController`, and `CardController` — the pre-Story-030 architecture. **None of those classes exist
> anymore.** This folder now documents their real, current replacements in `KOA.View`, `KOA.Controllers`, and
> `KOA.Logic`. Game rules live in `NetworkBoardState` (see [Networking](../networking/README.md)) — everything
> below is rendering and input only.

## Index

- [BoardView](./board-view.md) — renders the board, owns visual perspective
- [CardView](./card-view.md) — renders a single card (hand or board)
- [HandView](./hand-view.md) — lays out one player's hand
- [InputController](./input-controller.md) — mouse input → `NetworkBoardState.Cmd*` calls
- [TargetingHelper](./targeting-helper.md) — pure target-list calculation
- [EndTurnController](./end-turn-controller.md) — end-turn button

## Data Flow

```
Player clicks/drags (InputController)
       │
       ▼
NetworkBoardState.Cmd* (ServerRpc)         ← server validates & mutates BoardState
       │
       ▼
NetworkBoardState events (OnStateChanged, OnCardPlayed, ...)
       │
       ▼
BoardView.RenderBoard() / HandView.RenderHand()   ← re-render from state, no rules
```

There is no `PlayerController` — player state (`Health`, `Mana`, `Board`, `Hand`) lives in `PlayerBoardState`
inside `BoardState` (see [architecture.md](../architecture.md#model)); connection-level identity/stats live in
`NetworkPlayer` (see [Networking](../networking/README.md)).

---
*Parent: [Documentation](../README.md)*
