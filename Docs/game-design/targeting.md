# Targeting System

*Verified against source: 2026-08-12 (vdate) — rewritten to match `InputController`'s actual current behavior;
the previous version of this doc described a drag-destination scheme that isn't what's implemented.*

## Current Input Scheme

Kingdom Ov Animals uses click, not a single unified drag gesture. **Left-click** drives offensive/play actions;
**right-click** drives support actions — and they're not symmetric.

### Left-click (offensive / play)

| You click/drag | Drop target | Effect |
|-----------------|-------------|--------|
| Hand card | Empty own board slot | Play the unit (mana cost, gets summoning sickness) |
| Hand card | Enemy board card | Flip ability: deal the card's offensive-ability damage to that card, card stays in hand |
| Hand card | Opponent avatar | Flip ability: deal offensive-ability damage to the opponent directly |
| Board card (untapped, no sickness, not frozen) | Enemy board card | Attack that card |
| Board card (untapped, no sickness, not frozen) | Opponent avatar | Attack the opponent directly |

Using an ability from a board card taps it (one action per turn). Playing a card from hand gives it summoning
sickness until your next turn.

### Right-click (support)

Right-clicking your own actionable board card enters support-targeting mode; left-clicking a friendly board
card afterward uses that card's support ability. **There is currently no way to use a support ability from a
hand card** — `NetworkBoardState.CmdUseSupportAbility` requires the source to be a board card that `CanAct`.

## What Makes This Unique

Rather than separate "play" and "attack" phases, where you click/drop and *which* mouse button determines the
action. A card has no generic attack stat — its offensive ability defines what happens when it attacks, and its
defensive ability defines what its support action does.

## Not Yet Implemented

- Taunt / forced targeting
- Hand-card support abilities (see above)
- Any visual distinction beyond the single targeting line drawn during a drag

---
*Parent: [Game Design](./README.md)*
