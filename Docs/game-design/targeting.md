# Targeting System

## Overview

The targeting system is a core piece of Kingdom Ov Animals and what makes it unique. Rather than separate "play" and "attack" actions, everything is driven by **where you drag a card to**.

## From Hand - Drag to Target

When you have a card in your hand, you can drag it to one of three places, each with a different effect:

| Drag To | Effect | Cost |
|---------|--------|------|
| **Empty space** | Play the unit | Mana cost |
| **Friendly unit** | Use its defensive ability | Mana cost |
| **Opponent unit** | Use its offensive ability | Mana cost |

### Empty Space
Dropping a card on an empty space will **play the unit** for its mana cost. The unit enters the board with **summoning sickness** and cannot act until your next turn.

### Friendly Unit
Dropping a card on a friendly unit will **use its defensive ability** (support ability) for its mana cost. This lets you buff, heal, or support your own units directly from hand.

### Opponent Unit
Dropping a card on an opponent's unit will **use its offensive ability** for its mana cost. This lets you deal damage or apply effects to enemies directly from hand.

## On Board - Using Abilities

After a unit is played by dropping it on an empty space, it will have **summoning sickness**. On subsequent turns, the unit will be able to **use its ability once per turn** - similar to an attack in MTG or Hearthstone.

The key difference: rather than a generic attack, the unit can choose to use either:
- **Offensive ability** - Target an opponent's unit
- **Defensive ability** - Target a friendly unit

Once a unit uses an ability, it becomes **tapped** and cannot act again until the next turn.

## Flow Summary

```
┌─────────────────────────────────────────────────────────────┐
│                     CARD IN HAND                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   Drag to Empty Space ──────▶ PLAY UNIT                     │
│                               └─▶ Costs mana                │
│                               └─▶ Gets summoning sickness   │
│                                                             │
│   Drag to Friendly Unit ────▶ USE DEFENSIVE ABILITY         │
│                               └─▶ Costs mana                │
│                               └─▶ Card stays in hand (?)    │
│                                                             │
│   Drag to Opponent Unit ────▶ USE OFFENSIVE ABILITY         │
│                               └─▶ Costs mana                │
│                               └─▶ Card stays in hand (?)    │
│                                                             │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                     UNIT ON BOARD                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   Turn 1 (Played) ──────────▶ SUMMONING SICKNESS            │
│                               └─▶ Cannot act                │
│                                                             │
│   Turn 2+ ──────────────────▶ CAN USE ABILITY (once/turn)   │
│                               └─▶ Offensive OR Defensive    │
│                               └─▶ Becomes tapped            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## What Makes This Unique

In most card games like MTG or Hearthstone:
- Playing a card and attacking are separate actions
- Attacks are generic (creature vs creature combat)
- Abilities are often separate from attacks

In Kingdom Ov Animals:
- **One unified drag-and-drop system** handles everything
- **Where you drop determines the action**
- **Every unit has two distinct abilities** (offensive + defensive)
- **Abilities replace generic attacks** - more strategic variety
- **Hand cards can use abilities** before being played (at mana cost)

This creates more decision points: Do you play the unit now? Or use its ability from hand first? Which ability fits the current board state better?
