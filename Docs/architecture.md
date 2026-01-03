# Architecture Overview

## Core Controllers

### EncounterController
The main game orchestrator that manages:
- Player references (player & opponent)
- Hand controllers for both players
- Turn management and turn number tracking
- Game initialization and card drawing

**Key Methods:**
- `InitializeEncounter()` - Sets up the game, shuffles decks, draws initial cards
- `StartTurn()` - Handles turn start logic (mana refresh, card draw, board reset)
- `EndTurn()` - Switches active player and increments turn counter
- `DrawCard(PlayerController)` - Draws a card for the specified player

### PlayerController
Manages individual player state:
- Health and mana pools
- Deck and board card collections
- UI updates for player stats

**Key Methods:**
- `TakeDamage(int)` - Reduces player health
- `Heal(int)` - Restores player health
- `RefillMana()` - Restores mana to max at turn start
- `AddCardToBoard(CardController)` - Places a card on the board
- `RemoveCardFromBoard(CardController)` - Removes and destroys a card

### CardController
Represents individual cards with:
- Stats: name, mana cost, health
- Status flags: summoning sickness, tapped, frozen, buried, defending
- References to offensive and support abilities
- Owner reference to PlayerController

**Key Methods:**
- `TakeDamage(int)` - Reduces card health
- `Heal(int)` - Restores card health
- `TapCard()` / `UntapCard()` - Manages tapped state
- `SetSummoningSickness(bool)` - Controls summoning sickness
- `UpdateCardUI()` - Refreshes UI elements
- `UpdateVisualEffects()` - Updates status icons

### HandController
Manages the player's hand of cards:
- Card positioning in hand slots
- Drag and drop interactions
- Playing cards to the board
- Targeting line rendering

**Key Methods:**
- `AddCardToHand(CardController)` - Adds a card to the hand
- `RemoveCardFromHand(CardController)` - Removes a card from hand
- `VisualizePlayableHand()` - Highlights playable cards
- `VisualizePlayableBoard()` - Highlights usable board cards

### TargetingController
Handles target validation and selection:
- Determines valid targets based on ability type
- Tracks playable cards in hand
- Tracks usable cards on board

**Key Methods:**
- `GetPlayableCardsInHand()` - Returns cards player can afford to play
- `GetUsableCardsOnBoard()` - Returns cards that can use abilities
- `GetOffensiveTargets(CardController)` - Gets valid offensive targets
- `GetSupportTargets(CardController)` - Gets valid support targets

## Data Flow

```
EncounterController (Game State)
       │
       ├── PlayerController (Player 1)
       │       ├── Deck [CardController...]
       │       └── Board [CardController...]
       │
       ├── PlayerController (Player 2)
       │       ├── Deck [CardController...]
       │       └── Board [CardController...]
       │
       ├── HandController (Player 1 Hand)
       │       └── [CardController...]
       │
       └── HandController (Player 2 Hand)
               └── [CardController...]
```

## Component Relationships

```
CardController
    ├── offensiveAbility → AbilityController (e.g., DamageAbility)
    ├── supportAbility → AbilityController (e.g., Ability_Heal)
    └── owningPlayer → PlayerController

HandController
    ├── owningPlayer → PlayerController
    ├── encounterController → EncounterController
    └── targetingController → TargetingController

EncounterController
    ├── player → PlayerController
    ├── opponent → PlayerController
    ├── playerHandController → HandController
    └── opponentHandController → HandController
```
