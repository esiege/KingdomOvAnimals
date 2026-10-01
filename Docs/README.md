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
│   ├── Model/          # Pure data (BoardState, CardState) — single source of truth
│   ├── Network/         # FishNet sync (NetworkBoardState, NetworkGameManager, NetworkPlayer)
│   ├── View/             # Rendering + input (BoardView, CardView, HandView, InputController)
│   ├── Data/               # ScriptableObject templates (CardData, AbilityData, DeckData, CardLibrary)
│   ├── Logic/                # Pure stateless helpers (TargetingHelper)
│   ├── Abilities/              # Ability effect behaviors
│   ├── Controllers/              # EndTurnController (the last class left here)
│   └── Editor/                     # Editor-only tooling
├── Scenes/
│   ├── MainMenu.unity
│   ├── DuelScreen.unity        # Main game
│   ├── CardManagement.unity    # Card/ability/deck authoring
│   ├── Network Test.unity      # Connection smoke test
│   └── CHUDSandbox.unity
```

See [architecture.md](./architecture.md) for how these layers depend on each other.

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
Abilities are modular and support various targeting types (`KOA.Data.TargetType`):
- `Self` - Target self
- `SingleEnemy` - Target one enemy card
- `SingleAlly` - Target one friendly card
- `AllEnemies` - Target all enemy cards
- `AllAllies` - Target all friendly cards
- `EnemyPlayer` - Target the opponent player directly
- `BoardWide` - Affect all cards

## Documentation Index

*vdate = date content was last checked against source. Docs without a vdate note haven't been re-verified since
the Story 030/036 architecture revamp — read them skeptically.*

### Game Design
- [Game Design Overview](./game-design/README.md)
- [Game Flow](./game-design/game-flow.md) — vdate 2026-08-12
- [Card System](./game-design/cards.md) — vdate 2026-08-12
- [Ability System](./game-design/abilities.md) — vdate 2026-08-12
- [Targeting System](./game-design/targeting.md) — vdate 2026-08-12
- [Adventure Mode](./game-design/adventure-mode.md) — design vision, not implemented
- [Card Data Manager](./game-design/card-data-manager.md) — partially implemented, see banner

### Core Systems
- [Architecture Overview](./architecture.md) — vdate 2026-08-12
- [Utilities](./utilities.md) — vdate 2026-08-12

### Networking
- [Networking Overview](./networking/README.md) — vdate 2026-08-12
- [NetworkBoardState](./networking/network-board-state.md) — vdate 2026-08-12
- [NetworkGameManager](./networking/network-game-manager.md) — vdate 2026-08-12
- [NetworkPlayer](./networking/network-player.md) — vdate 2026-08-12
- [Turn Synchronization](./networking/turn-synchronization.md) — vdate 2026-08-12
- [Reconnection](./networking/reconnection.md) — vdate 2026-08-12

### Controllers / View
- [Overview](./controllers/README.md) — vdate 2026-08-12
- [BoardView](./controllers/board-view.md), [CardView](./controllers/card-view.md),
  [HandView](./controllers/hand-view.md), [InputController](./controllers/input-controller.md),
  [TargetingHelper](./controllers/targeting-helper.md), [EndTurnController](./controllers/end-turn-controller.md)

### Troubleshooting (historical incident logs — see folder README before trusting class names)
- [Troubleshooting Index](./troubleshooting/README.md)
- [Reconnection Debugging](./troubleshooting/reconnection-debugging.md)
- [Turn Sync Issues](./troubleshooting/turn-sync-issues.md)
- [DontDestroyOnLoad Issues](./troubleshooting/dont-destroy-on-load.md)

## Getting Started

1. Open the project in Unity 2021.3.10f1
2. Open the `DuelScreen` scene for the main game
3. Use `Network Test` scene for multiplayer connection testing, or **KOA → Testing → Rebuild SceneIds + Build +
   Run** for a full two-instance test (see [CLAUDE.md](../CLAUDE.md))

## Development Status

🚧 **In Development** — networking/reconnection work is actively in progress as of vdate.

---
*Last updated: 2026-08-12*
