# Story 044: Adventure Mode Design Decisions

## Status: Not Started
## Sprint: 07
## Dependencies: None
## Created: 2026-08-12

## Background

`Docs/game-design/adventure-mode.md` has an explicit "Open Tensions (not blockers)" section that was
deliberately left unresolved during early ideation. Before Sprint 08 onward can build against a stable design,
these need actual decisions — not more brainstorming.

## User Story
**As a** developer
**I want** the open Adventure Mode design tensions resolved
**So that** Sprints 08+ build against one plan instead of a moving target

## Acceptance Criteria
- [ ] Decide: second type is random vs. player-chosen after Board 1
- [ ] Decide: pure PvP (no NPC fallback) vs. AI-opponent fallback when no player is queued at a given run
      position — informed by Story 045's matchmaking spike results
- [ ] Decide: exact board/segment/win counts if they still need adjusting (currently 3 boards × 3 segments + 1
      boss = 10 duels, 3 lives)
- [ ] `Docs/game-design/adventure-mode.md` updated to reflect the decisions, tensions section removed or resolved
- [ ] `Docs/scrum/backlog.md` and any downstream stories (028, etc.) updated if a decision changes their scope

## Notes
This is a design/decision story, not a code story — output is documentation, not a PR.
