# Kingdom Ov Animals

A multiplayer card dueling game built in Unity with FishNet networking.

## Project Overview

**Kingdom Ov Animals** is a turn-based card game where players summon creatures with unique abilities to battle opponents. The game features a mana system, card abilities (offensive and support), and various status effects.

## Tech Stack

- **Engine**: Unity
- **Networking**: FishNet
- **UI**: TextMesh Pro
- **Language**: C#

## Project Structure

```
Assets/
├── Scripts/
│   ├── Abilities/       # Card ability system
│   ├── Controllers/     # Core game logic
│   └── Effects/         # Visual/hover effects
├── Prefabs/
│   └── Card/            # Card prefabs and ability prefabs
├── Scenes/
│   ├── CHUDSandbox.unity
│   ├── DuelScreen.unity
│   └── Network Test.unity
├── Images/
├── Music/
└── Resources/
```

## Core Systems

### Card System
Cards are the primary gameplay element with the following properties:
- **Name, Mana Cost, Health**
- **Offensive & Support Abilities**
- **Status Effects**: Summoning sickness, frozen, buried, defending, tapped

### Player System
Each player has:
- Health / Max Health
- Mana / Max Mana
- Deck and Board collections

### Turn System
- Players take alternating turns
- Mana increases each turn (+1 max mana)
- Cards have summoning sickness on the turn they're played

### Ability System
Abilities are modular and support various targeting types:
- `SingleEnemy` - Target one enemy card
- `SingleFriendly` - Target one friendly card
- `AllEnemies` - Target all enemy cards
- `AllFriendlies` - Target all friendly cards
- `Self` - Target self
- `PlayerOnly` - Target opponent player directly
- `BoardWide` - Affect all cards

## Documentation Index

### Game Design
- [Game Design Overview](./game-design/README.md)
- [Game Flow](./game-design/game-flow.md)
- [Card System](./game-design/cards.md)
- [Ability System](./game-design/abilities.md)
- [Targeting System](./game-design/targeting.md)

### Core Systems
- [Architecture Overview](./architecture.md)
- [Utilities](./utilities.md)

### Networking
- [Networking Overview](./networking/README.md)
- [NetworkGameManager](./networking/network-game-manager.md)
- [PlayerConnectionHandler](./networking/player-connection-handler.md)
- [NetworkPlayer](./networking/network-player.md)
- [Turn Synchronization](./networking/turn-synchronization.md)
- [Reconnection](./networking/reconnection.md)
- [State Snapshot](./networking/state-snapshot.md)

### Controllers
- [Controllers Overview](./controllers/README.md)
- [EncounterController](./controllers/encounter-controller.md)
- [PlayerController](./controllers/player-controller.md)
- [HandController](./controllers/hand-controller.md)
- [TargetingController](./controllers/targeting-controller.md)
- [CardController](./controllers/card-controller.md)
- [EndTurnController](./controllers/end-turn-controller.md)

### Troubleshooting
- [Troubleshooting Index](./troubleshooting/README.md)
- [Reconnection Debugging](./troubleshooting/reconnection-debugging.md)
- [Turn Sync Issues](./troubleshooting/turn-sync-issues.md)
- [DontDestroyOnLoad Issues](./troubleshooting/dont-destroy-on-load.md)

## Getting Started

1. Open the project in Unity (version TBD)
2. Open the `DuelScreen` scene for the main game
3. Use `Network Test` scene for multiplayer testing

## Development Status

🚧 **In Development**

---
*Last updated: January 2026*
