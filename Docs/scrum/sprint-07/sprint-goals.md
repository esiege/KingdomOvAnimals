# Sprint 07 Goals

*(planned 2026-08-12, not yet started)*

## Primary Goal

**Turn the Adventure Mode pitch's open tensions into locked decisions, informed by evidence rather than more
brainstorming.**

## Success Criteria

By the end of this sprint, we should have:

1. [ ] Random-vs-chosen second type decided (Story 044)
2. [ ] AI-fallback-vs-pure-PvP decided, backed by a real matchmaking spike rather than a guess (Stories 044, 045)
3. [ ] `Docs/game-design/adventure-mode.md`'s "Open Tensions" section resolved or removed

## Key Focus Areas

### 1. Design Decisions
This is a documentation/decision sprint, not primarily a code sprint — the deliverable is a settled design that
Sprints 08+ can build against without the ground shifting under them.

### 2. Matchmaking Spike
"No NPC fights" only works if matchmaking-by-run-position is actually viable at realistic player counts. Spike
this before committing the run/map architecture (Sprint 08) to assuming it works.

## Non-Goals (Out of Scope)

- Building the actual run/map state machine (Sprint 08 — depends on this sprint's decisions)
- Production-quality matchmaking code (Story 045's spike is explicitly throwaway; Story 053 in Sprint 16 hardens
  it later)

## Definition of Done for Sprint

- [ ] `adventure-mode.md` reflects locked decisions, not open questions
- [ ] Matchmaking spike has a written finding, informing the AI-fallback decision
- [ ] Any downstream stories (028, etc.) updated if a decision changed their scope
