# Game Flow

## Match Initialization

```
EncounterController.Start()
       │
       ▼
InitializeEncounter()
       │
       ├── Set currentPlayer = player (Player 1 goes first)
       ├── player.ShuffleDeck()
       └── opponent.ShuffleDeck()
       │
       ▼
DrawInitialCards() [Coroutine]
       │
       ├── Draw 3 cards for Player 1 (0.1s delay each)
       └── Draw 3 cards for Player 2 (0.1s delay each)
       │
       ▼
StartTurn()
```

## Turn Structure

### Start of Turn
```
StartTurn()
       │
       ├── Set currentPlayer
       ├── Increment max mana (+1)
       ├── RefillMana() - Restore mana to max
       ├── ResetBoard() - Clear summoning sickness & tap status
       ├── DrawCard(currentPlayer)
       │
       └── Update UI
           ├── HideBoardTargets()
           ├── HidePlayableHand()
           ├── HidePlayableBoard()
           ├── VisualizePlayableHand()
           └── VisualizePlayableBoard()
```

### During Turn (Player Actions)

#### Playing a Card from Hand
```
1. Click card in hand
       │
       ▼
2. Check: currentMana >= card.manaCost
       │
       ▼
3. Check: Board has open slot
       │
       ▼
4. Drag card to board slot
       │
       ▼
5. Execute:
   ├── Deduct mana cost
   ├── Remove card from hand
   ├── Add card to board
   ├── Set card.isInPlay = true
   ├── Set card.hasSummoningSickness = true
   └── Update visuals
```

#### Using a Card's Ability
```
1. Click card on board
       │
       ▼
2. Check card can act:
   ├── owningPlayer == currentPlayer
   ├── isInPlay == true
   ├── hasSummoningSickness == false
   └── isTapped == false
       │
       ▼
3. Get valid targets (TargetingController)
       │
       ▼
4. Highlight valid targets
       │
       ▼
5. Player selects target
       │
       ▼
6. Execute ability
       │
       ▼
7. Tap the card (isTapped = true)
```

### End of Turn
```
EndTurn()
       │
       ├── Toggle isCurrentPlayerTurn
       ├── Increment turnNumber
       │
       └── StartTurn() for next player
```

## Mana Economy

| Turn | Max Mana |
|------|----------|
| 1 | 1 |
| 2 | 2 |
| 3 | 3 |
| ... | ... |
| N | N |

- Mana refills to max at the start of each turn
- Playing cards costs mana equal to their `manaCost`

## Win Conditions

*(To be documented - likely player health reaching 0)*

## State Machine Overview

```
┌─────────────────────────────────────────────────────────┐
│                    MATCH STATES                         │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌──────────┐    ┌──────────────┐    ┌──────────────┐  │
│  │  SETUP   │───▶│ PLAYER TURN  │───▶│ OPPONENT     │  │
│  │          │    │              │    │ TURN         │  │
│  └──────────┘    └──────────────┘    └──────────────┘  │
│                         ▲                    │          │
│                         └────────────────────┘          │
│                                                         │
│  Victory/Defeat when player health <= 0                 │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

```
┌─────────────────────────────────────────────────────────┐
│                    TURN PHASES                          │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌──────────┐    ┌──────────────┐    ┌──────────────┐  │
│  │  START   │───▶│    MAIN      │───▶│     END      │  │
│  │  PHASE   │    │    PHASE     │    │    PHASE     │  │
│  └──────────┘    └──────────────┘    └──────────────┘  │
│                                                         │
│  Start: Draw, Mana, Reset                               │
│  Main: Play cards, Use abilities                        │
│  End: Switch player                                     │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

## Hand Management

- Maximum hand size: 5 cards (configurable via `maxHandSize`)
- Cards drawn when hand is full: *(behavior TBD)*
- Card positions managed by `cardPositions` list in HandController
