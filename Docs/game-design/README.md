# Game Design

Documentation for Kingdom Ov Animals game rules, mechanics, and design decisions.

## Contents

| Document | Description |
|----------|-------------|
| [abilities.md](abilities.md) | Ability system, effect types, and composable effects |
| [adventure-mode.md](adventure-mode.md) | Adventure Mode design - deck building through narrative |
| [cards.md](cards.md) | Card properties, stats, state flags, and visual elements |
| [game-flow.md](game-flow.md) | Match initialization, turn structure, and player actions |
| [targeting.md](targeting.md) | Unique drag-to-target system that defines gameplay |
| [card-data-manager.md](card-data-manager.md) | Data entry scene for managing cards and abilities |

## Core Concepts

### The Targeting System

Kingdom Ov Animals uses a unique **drag-to-target** system that combines playing cards and using abilities into a single intuitive action:

- **Drag to empty space** → Play the unit (costs mana, gets summoning sickness)
- **Drag to friendly unit** → Use defensive/support ability (costs mana)
- **Drag to enemy unit** → Use offensive ability (costs mana)

This eliminates the need for separate "play" and "attack" phases found in traditional card games.

### Turn Structure

1. **Start of Turn**: +1 max mana, refill mana, draw card, reset board
2. **Main Phase**: Play cards, use abilities (in any order)
3. **End Turn**: Pass to opponent

### Card Abilities

Each card has two ability slots:
- **Offensive Ability** - Used against enemies (damage, debuffs)
- **Support Ability** - Used on friendlies (heals, buffs)

Units on board can use ONE ability per turn (tapped after use).

---
*Parent: [Documentation](../README.md)*
