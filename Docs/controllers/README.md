# Controllers

Core game logic controllers that manage gameplay mechanics.

## Index

- [EncounterController](./encounter-controller.md) - Main game orchestration
- [PlayerController](./player-controller.md) - Individual player state
- [HandController](./hand-controller.md) - Hand management and card interactions
- [TargetingController](./targeting-controller.md) - Target validation
- [CardController](./card-controller.md) - Individual card behavior
- [EndTurnController](./end-turn-controller.md) - Turn ending UI

## Controller Hierarchy

```
EncounterController (Scene Root)
├── PlayerController (Player)
│   └── NetworkPlayer (runtime link)
├── PlayerController (Opponent)
│   └── NetworkPlayer (runtime link)
├── HandController (Player Hand)
├── HandController (Opponent Hand)
└── TargetingController
```

## Responsibilities

| Controller | Primary Function |
|------------|------------------|
| EncounterController | Turn flow, game initialization, player coordination |
| PlayerController | Health, mana, deck, board management |
| HandController | Card selection, drag-drop, ability activation |
| TargetingController | Valid target calculation |
| CardController | Individual card state and visuals |

## Network Integration

In multiplayer mode:
- Controllers receive state changes from NetworkPlayer SyncVars
- User actions send ServerRpc through NetworkPlayer
- Visual updates triggered by ObserversRpc broadcasts
