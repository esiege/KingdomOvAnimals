# Sprint 10 Goals

*(planned 2026-08-12, not yet started)*

## Primary Goal

**Build the non-combat segment nodes — what a player actually does between duels.**

## Success Criteria

By the end of this sprint, we should have:

1. [ ] Story branch data model supporting narrative + 2-4 choices (Story 029)
2. [ ] Story presentation UI (Story 030)
3. [ ] Card pack rewards tied to story choices (Story 031)
4. [ ] AI-generated story branch content, with template/fallback content for reliability (Story 035)

## Key Focus Areas

### 1. This Is the Core of "What Are Map Nodes"
Since Adventure Mode has no PvE fights, these segment nodes are the entire non-combat content of the mode — this
sprint is arguably the most content-critical one in the whole plan.

### 2. AI Generation With a Safety Net
Story 035 needs template/fallback branches per type so a generation failure doesn't block a run.

## Non-Goals (Out of Scope)

- Card specialization/upgrades (Sprint 13)
- AI-generated *cards* (a separate, higher-risk system — Sprint 15)
- Wiring these nodes into the run state machine's board/segment progression (Sprint 08 built the state machine;
  confirm the two integrate, but deep wiring is Sprint 11's job)

## Definition of Done for Sprint

- [ ] A full segment (story branch → choice → card reward) is playable end-to-end
- [ ] Generation failures gracefully fall back to template content, don't block the player
