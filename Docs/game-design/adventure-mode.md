# Adventure Mode Design

> **Not implemented — active design vision, early ideation.** *(vdate 2026-08-12)* No story-branch,
> class-selection, or progression code exists anywhere in `Assets/Scripts`. This doc is being actively
> re-pitched and will keep shifting; details below are the current best guess, not locked decisions. The only
> scenes that currently exist are `MainMenu`, `DuelScreen`, `Network Test`, `CardManagement`, and `CHUDSandbox`
> (see [CLAUDE.md](../../CLAUDE.md)).

## Pitch

Hearthstone Duels' draft-a-run structure, crossed with Slay the Spire's map traversal and deckbuilding-through-a-run,
built on top of this game's existing 1v1 duel combat. Players walk a branching board, making narrative/reward
choices that grow their deck, and periodically stop to duel another live player. No NPC/PvE fights — every
battle in the run is against a real opponent.

## Run Structure

```
┌──────────────────────────────────────────────────────────────────┐
│  START: pick a starter type (taxonomic class/phylum), get its    │
│  starter deck                                                    │
│         ↓                                                        │
│  BOARD 1 (random type)      — 3 segments, each ends in a PvP duel│
│         ↓  (after Board 1: get a second, random type added to    │
│             the deck — e.g. start Aves/birds, pick up             │
│             Cnidaria/jellyfish)                                  │
│  BOARD 2 (random type)      — 3 segments, each ends in a PvP duel│
│         ↓                                                        │
│  BOARD 3 (fixed "strange" capstone board, not a normal type)     │
│                              — 3 segments, each ends in a PvP duel│
│         ↓                                                        │
│  FINAL BOSS DUEL            — the 10th and last PvP battle       │
└──────────────────────────────────────────────────────────────────┘
```

- 3 boards × 3 segments = 9 segment-ending duels + 1 final boss duel = **10 total PvP battles per run**
- **3 lives**, lost only on a PvP loss (not on non-combat choices — there's nothing else to fail at)
- 0 lives = run over

## Type System

Starter type and the Board-2 type are each a taxonomic grouping — rank is deliberately inconsistent for content
variety (a whole phylum like Cnidaria can sit alongside a class like Aves, and a large class like Mammalia can
be split into finer family-level groups like Canidae/Felidae/Ursidae for more distinct options). This reuses
and extends the existing `Docs/scrum/sprint-04/data/animal-classes.json` (107 Family-level groups already
written) rather than starting from nothing — see [Prior Work](#prior-work) below.

Deck mixing between the starter type and the Board-2 type is the core replayability hook: same starting type,
different second type, meaningfully different deck every run. Stretch goal: AI-generated "mutation" cards that
blend the two active types for extra build variety (pairs naturally with the AI story generation groundwork
already scoped in `story-035`, just applied to card generation instead of narrative text).

## Segment Flow (non-combat nodes)

Between duels, each segment presents story-branch-style choices — this part of the old plan still fits well:

1. **Narrative text** — atmospheric scenario description
2. **Choices** (2-4 options) — each grants different card rewards
3. **Card rewards** — packs of cards added to the deck
4. **Specializations** (occasional) — upgrade an existing card down a themed path

Example (unchanged from the earlier draft — still a good template):

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
───────────────────────────────────────────────────────────────
Choice 2: "Observe from the shadows"
    Reward: 2x Wolf + 1x Lone Wolf (rare)
───────────────────────────────────────────────────────────────
Choice 3: "Challenge the alpha for leadership"
    Reward: 3x Wolf + 1x Alpha Wolf (legendary)
───────────────────────────────────────────────────────────────
```

Specialization choice example:

```
═══════════════════════════════════════════════════════════════
THE WOLF RISES
═══════════════════════════════════════════════════════════════
Choose a path of power:

🔮 MAGE PATH      Wolf → Wolf Mage      +Magic abilities, -1 Health
⚔️ FIGHTER PATH   Wolf → Wolf Warrior   +Attack damage, balanced stats
🛡️ GUARDIAN PATH  Wolf → Wolf Guardian  +Health, defensive abilities
───────────────────────────────────────────────────────────────
```

## Duels

- Every duel is PvP — no AI/NPC opponents, by design
- Ideally matched against players at the same run position (same board/segment) so deck power stays comparable
- Loss = -1 life, continue to the next segment either way (run only ends at 0 lives)

## Open Tensions (not blockers — noted so they don't get lost)

*Formal resolution tracked as [Story 044](../scrum/sprint-07/todo/story-044-adventure-mode-design-decisions.md),
Sprint 07 — these stay open until then.*

- **Second type: random vs. chosen.** Current pitch says random. The older sprint-04 plan had the player choose
  their second class. Either could work; not decided.
- **No AI/NPC fallback removes a liveness safety valve.** The older plan allowed an AI opponent when no player
  was queued at your run position. The current pitch cuts NPC fights entirely, which means matchmaking has to
  actually work at every board/segment combination for the run to be playable. [Story 045](../scrum/sprint-07/todo/story-045-matchmaking-by-run-position-spike.md)
  spikes this before the decision is locked.
- **Board count/win total changed.** Old plan: 2 boards + a loosely-sized "endgame" (~12 total wins). Current
  pitch: clean 3 boards × 3 segments + 1 boss = 10. Going with the new number for now.
- **Category taxonomy needs a pass.** The existing `animal-classes.json` categories don't cleanly support
  "Cnidaria as a marquee board type" yet — jellyfish/cnidarians are currently folded into a generic "Other"
  bucket alongside octopuses and crabs. Fine to leave as-is until board-type selection is actually built.

## Prior Work

A meaningful chunk of this was already scoped in the old Sprint 04 and has since been rescheduled into
Sprints 08-13 (see `Docs/scrum/backlog.md`) rather than rebuilt from scratch:

- [story-027: Animal Classification Data](../scrum/sprint-09/todo/story-027-animal-classification-data.md) +
  [animal-classes.json](../scrum/sprint-04/data/animal-classes.json) — 107 Family-level groups already written
- [story-028: Class Selection UI](../scrum/sprint-09/todo/story-028-class-selection-ui.md)
- [story-029: Story Branch Data](../scrum/sprint-10/todo/story-029-story-branch-data.md) /
  [story-030: Story Presentation UI](../scrum/sprint-10/todo/story-030-story-presentation-ui.md)
- [story-031: Card Pack Reward](../scrum/sprint-10/todo/story-031-card-pack-reward.md) /
  [story-032: Card Specialization](../scrum/sprint-13/todo/story-032-card-specialization.md)
- [story-033: Adventure Progress](../scrum/sprint-08/todo/story-033-adventure-progress.md) /
  [story-034: Board Map UI](../scrum/sprint-08/todo/story-034-board-map-ui.md)
- [story-035: AI Story Generation](../scrum/sprint-10/todo/story-035-ai-story-generation.md) — template for the
  AI-generated mutation-card idea (now its own spike, [story-052](../scrum/sprint-15/todo/story-052-ai-generated-mutation-cards-spike.md))

---
*Parent: [Game Design](./README.md)*
