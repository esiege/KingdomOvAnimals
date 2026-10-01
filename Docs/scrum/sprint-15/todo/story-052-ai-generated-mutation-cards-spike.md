# Story 052: AI-Generated Mutation Cards (Spike)

## Status: Not Started
## Sprint: 15
## Dependencies: 047
## Created: 2026-08-12

## Background

Stretch idea from the design pitch: generate hybrid cards blending the player's two active types (e.g. Aves ×
Cnidaria) for extra build variety. Related in spirit to Story 035 (AI story generation) but a separate system —
that one generates narrative text, this one would need to generate balanced card data (stats + ability
selection), which is a much higher-risk content-generation problem.

## User Story
**As a** player
**I want** occasional unique hybrid cards blending my two types
**So that** each run's card pool feels distinct beyond the fixed type card pools

## Acceptance Criteria
- [ ] Spike: can a prompted generation step produce a `CardData`-shaped result (name, cost, health, ability
      references) that's mechanically valid without manual cleanup?
- [ ] Balance guardrails: generated cards constrained to reasonable stat/cost ranges relative to existing cards
- [ ] Written finding on whether this is viable for real content or needs a curated-template fallback instead
- [ ] Explicitly out of scope for this story: shipping this as a live feature — this is a feasibility spike

## Notes
Easiest story in the whole plan to cut or slip if 05-14 run long — nothing downstream depends on it.
