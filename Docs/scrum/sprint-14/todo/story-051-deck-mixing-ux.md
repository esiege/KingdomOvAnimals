# Story 051: Deck-Mixing UX

## Status: Not Started
## Sprint: 14
## Dependencies: 047
## Created: 2026-08-12

## Background

Mixing the starter type and the Board-2 type into one deck is called out in the design doc as the core
replayability hook of the whole mode. It deserves dedicated UX attention rather than being a side effect of
Story 047's data model.

## User Story
**As a** player
**I want** to clearly see and understand how my two types are blending in my deck
**So that** the build-crafting decision space is legible, not just "cards got added"

## Acceptance Criteria
- [ ] Deck viewer visually distinguishes cards by their originating type
- [ ] Player can see deck composition (type balance, mana curve) at any point during the run
- [ ] New cards from the second type are called out distinctly when first added (post-Board-1 transition)

## Notes
UI/UX-focused story; depends on whatever deck viewer conventions exist from `CardManagement`'s `DeckEditorUI`.
