# Story 045: Matchmaking-by-Run-Position Spike

## Status: Not Started
## Sprint: 07
## Dependencies: None
## Created: 2026-08-12

## Background

If Adventure Mode has no NPC/AI fallback (current pitch), every one of the run's 10 duels needs a live opponent
at the *same* run position (board/segment) queued at the *same* time. This is a throwaway spike to find out
whether that's actually viable before committing the run/map architecture (Story 046) to it.

## User Story
**As a** developer
**I want** to know whether pure-PvP matchmaking is viable at low concurrent player counts
**So that** Story 044's AI-fallback decision is based on evidence, not guesswork

## Acceptance Criteria
- [ ] Prototype (throwaway code acceptable) a matchmaking queue keyed by board/segment position
- [ ] Test/simulate with a small number of concurrent "players" (bots driving `ConnectionManager`, or manual
      multi-instance testing via `KOA → Testing → Rebuild SceneIds + Build + Run`) to estimate queue times
- [ ] Written finding: is a queue-timeout fallback (bot opponent, ghost of a past run, or accept the wait)
      needed, and if so which
- [ ] Feed the result into Story 044's AI-fallback decision

## Notes
This story's *output* is a finding, not necessarily shippable code — don't over-invest in production quality.
