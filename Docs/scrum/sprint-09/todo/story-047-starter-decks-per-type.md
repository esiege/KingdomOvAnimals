# Story 047: Starter Decks Per Type

## Status: Not Started
## Sprint: 09
## Dependencies: 027
## Created: 2026-08-12

## User Story
**As a** player
**I want** a themed starter deck when I pick my starting type
**So that** my run feels like "Aves/birds" or "Cnidaria/jellyfish" from the first duel, not a generic deck

## Acceptance Criteria
- [ ] Each type in the (amended, Story 027) classification data has an associated starter `DeckData` or
      card-pool definition
- [ ] Starter deck is legal-sized and playable standalone (matches whatever deck-size rules `story-024`'s deck
      selection settles on)
- [ ] When the second type is added after Board 1, its cards integrate into the existing deck rather than
      replacing it

## Notes
Depends on the type-mixing amendment in Story 027 and the random-vs-chosen decision in Story 044.
