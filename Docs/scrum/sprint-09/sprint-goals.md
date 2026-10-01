# Sprint 09 Goals

*(planned 2026-08-12, not yet started)*

## Primary Goal

**Get the type/classification system and starter content ready for players to actually pick a type and get a
themed deck.**

## Success Criteria

By the end of this sprint, we should have:

1. [ ] Classification data extended for taxonomic-rank mixing — a phylum like Cnidaria alongside a class like
      Aves, Mammalia split into finer groups (Story 027, amended)
2. [ ] Cnidaria/jellyfish promoted out of the existing `animal-classes.json`'s generic "Other" bucket
3. [ ] Starter-type selection UI (Story 028, amended for Sprint 07's random-vs-chosen decision)
4. [ ] A themed starter deck per type (Story 047)

## Key Focus Areas

### 1. Extending Existing Data
The 107 Family-level groups in `Docs/scrum/sprint-04/data/animal-classes.json` are a real head start — this is
extension, not a rewrite from scratch.

### 2. Starter Decks
Each type needs a legal, standalone-playable starter deck that also integrates cleanly when the second type gets
added post-Board-1.

## Non-Goals (Out of Scope)

- Story branches / card pack rewards beyond the starter deck (Sprint 10)
- The second-type addition flow itself if Sprint 07 decided it's random (that's just data selection, not new UI)

## Definition of Done for Sprint

- [ ] Player can select a starter type and see its themed starter deck
- [ ] Classification data supports mixed taxonomic ranks without special-casing in code
