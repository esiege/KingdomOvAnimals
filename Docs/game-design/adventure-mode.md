# Adventure Mode Design

## Overview

Adventure Mode is the primary gameplay experience in Kingdom Ov Animals. Players embark on a deck-building journey through narrative choices and duels, starting from a single animal class and evolving into a multi-class powerhouse.

## Core Gameplay Loop

```
┌─────────────────────────────────────────────────────────────┐
│                     ADVENTURE MODE                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. CLASS SELECTION                                         │
│     Choose from 107 animal classes                          │
│     ↓                                                       │
│  2. BOARD 1 (Primary Class)                                 │
│     ┌─────────────────────────────────────────────┐        │
│     │ Story Branch → Duel → Story → Duel → Story → Duel │  │
│     │    (+cards)    (W/L)  (+cards) (W/L) (+cards) (W/L)│  │
│     └─────────────────────────────────────────────┘        │
│     ↓                                                       │
│  3. SECONDARY CLASS SELECTION                               │
│     Choose a second class to complement your deck           │
│     ↓                                                       │
│  4. BOARD 2 (Secondary Class)                               │
│     ┌─────────────────────────────────────────────┐        │
│     │ Story Branch → Duel → Story → Duel → Story → Duel │  │
│     │    (+cards)    (W/L)  (+cards) (W/L) (+cards) (W/L)│  │
│     └─────────────────────────────────────────────┘        │
│     ↓                                                       │
│  5. ENDGAME                                                 │
│     Final challenges, rewards, completion                   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Story Branches

### Structure
Each story branch presents:
1. **Narrative Text** - Atmospheric description of a scenario
2. **Choices** (2-4 options) - Each with different rewards
3. **Card Rewards** - Packs of cards added to deck
4. **Specializations** - Optional card upgrades

### Example Story Branch

```
═══════════════════════════════════════════════════════════════
THE CALL OF THE PACK
═══════════════════════════════════════════════════════════════

The moon rises over the forest. In the distance, you hear 
howling - a pack of wolves on the hunt. Their eyes gleam in 
the darkness as they notice your presence...

───────────────────────────────────────────────────────────────
Choice 1: "Join the hunt alongside them"
    Reward: 4x Wolf
    → Path: The pack accepts you as one of their own
───────────────────────────────────────────────────────────────
Choice 2: "Observe from the shadows"
    Reward: 2x Wolf + 1x Lone Wolf (rare)
    → Path: You learn their ways through careful observation
───────────────────────────────────────────────────────────────
Choice 3: "Challenge the alpha for leadership"
    Reward: 3x Wolf + 1x Alpha Wolf (legendary)
    → Path: A bold move that could change everything
───────────────────────────────────────────────────────────────
```

### Specialization Choices

After certain story branches, players can specialize their cards:

```
═══════════════════════════════════════════════════════════════
THE WOLF RISES
═══════════════════════════════════════════════════════════════

One of your wolves has shown exceptional talent. Under the 
moonlight, they await your guidance to unlock their potential...

Choose a path of power:

───────────────────────────────────────────────────────────────
🔮 MAGE PATH
    Wolf → Wolf Mage
    +Magic abilities, -1 Health
    "Channels lunar energy into devastating spells"
───────────────────────────────────────────────────────────────
⚔️ FIGHTER PATH
    Wolf → Wolf Warrior
    +Attack damage, balanced stats
    "Raw strength honed through countless battles"
───────────────────────────────────────────────────────────────
🛡️ GUARDIAN PATH
    Wolf → Wolf Guardian
    +Health, defensive abilities
    "An unbreakable protector of the pack"
───────────────────────────────────────────────────────────────
```

## Progression System

### Lives
- Players start with **3 lives**
- Losing a duel costs **1 life**
- At **0 lives**, the adventure ends
- Lives do NOT regenerate between boards

### Win Tracking
- **Board 1**: 3 duels (0-3 wins possible)
- **Board 2**: 3 duels (3-6 wins possible)
- **Endgame**: Final challenges (6-12 wins)
- **12 wins** = Complete victory

### Difficulty Scaling
- Early duels: Weaker opponent decks
- Later duels: Stronger, more synergistic decks
- After losses: Slightly easier matchmaking (optional mercy rule)

## Deck Building

### Starting Deck
- 0 cards initially
- First story choice grants ~4 cards
- Minimum deck size for first duel: ~12 cards (3 branches × 4 cards)

### Deck Growth
| Phase | Cards Added | Total (Approx) |
|-------|-------------|----------------|
| Class Selection | 0 | 0 |
| Story 1 | 4 | 4 |
| Story 2 | 4 | 8 |
| Story 3 | 4 | 12 |
| Story 4 | 4 | 16 |
| Story 5 | 4 | 20 |
| Story 6 | 4 | 24 |

### Multi-Class Decks
- **Board 1**: Single animal class only
- **Board 2**: Primary + Secondary class
- Synergies between classes encouraged through story context

## Animal Classes

### Categories
| Category | Count | Examples |
|----------|-------|----------|
| Mammals | 55 | Wolves, Bears, Elephants |
| Birds | 21 | Eagles, Owls, Penguins |
| Reptiles | 17 | Crocodiles, Snakes, Chameleons |
| Amphibians | 6 | Frogs, Salamanders |
| Fish | 7 | Sharks, Tuna, Salmon |
| Insects | 10 | Ants, Bees, Butterflies |
| Arachnids | 2 | Tarantulas, Scorpions |
| Other | 4 | Octopuses, Crabs, Jellyfish |

### Class Identity
Each class has:
- **Playstyle theme** (aggro, control, combo, etc.)
- **Visual identity** (card art style)
- **Ability keywords** (pack synergy, venom, flight, etc.)

## AI Story Generation

### Context Variables
The AI receives:
- `{className}` - Selected animal class
- `{classDescription}` - Class flavor text
- `{deckSummary}` - Current deck composition
- `{lastChoice}` - Previous player decision
- `{battleResult}` - Win/loss from last duel
- `{winsTotal}` - Cumulative wins
- `{currentBoard}` - Board 1 or 2

### Prompt Strategy
1. Maintain narrative consistency with previous choices
2. Reference cards the player has collected
3. Offer meaningful strategic choices (not just flavor)
4. Ensure card rewards exist in the library

### Fallback Content
- Template branches for each animal class
- Generic branches that work with any class
- Error handling with graceful degradation

## Duels

### Matchmaking
- Adventure players matched with other adventure players
- Similar progress (Board 1 vs Board 1)
- Similar deck sizes
- Optional: AI opponents if no players available

### Rewards
- **Win**: Progress to next story branch
- **Lose**: Lose 1 life, continue to next story branch

### Post-Duel Narrative
Story branches after duels reference the outcome:
- "Your victory echoes through the forest..."
- "Despite the defeat, your pack's spirit remains unbroken..."

## Endgame

### After 6 Wins (Both Boards Complete)
- Boss-tier challenges
- Rare card rewards
- Unique story conclusions

### 12-Win Completion
- Adventure victory screen
- Rewards unlocked
- Deck saved as "Hall of Fame" entry

## UI/UX Considerations

### Board Map
- Visual path showing progress
- Node types: Story, Duel, Class Selection
- Completed/current/future state visibility

### Deck Viewer
- Always accessible during story phases
- Shows all collected cards
- Highlights new additions

### Timer Considerations
- Story choices: No timer (thoughtful decisions)
- Duels: Standard turn timer

---

*See also: [Story Branch Data](../scrum/sprint-04/story-029-story-branch-data/story.md) | [Animal Classes JSON](../scrum/sprint-04/data/animal-classes.json)*
