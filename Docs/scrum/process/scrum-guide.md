# Scrum Process Guide

This document describes how we run Scrum for the Game Master Assistant project.

## Overview

We use a lightweight Scrum approach adapted for solo/small team development. The focus is on clear documentation, incremental delivery, and maintaining flexibility.

## Sprint Duration

**2 weeks** (10 working days)

## Scrum Artifacts

### 1. Product Backlog
High-level list of features and improvements maintained in `NEXT_STEPS.md` or a dedicated backlog file.

### 2. Sprint Backlog
Stories selected for the current sprint, documented in `sprint-XX/sprint-planning.md`

### 3. Story Documentation
Each story has its own folder under the sprint folder with:
- `README.md` - Story details, acceptance criteria, tasks
- `technical-notes.md` - Implementation details, decisions
- `testing.md` - Test scenarios and results

## Sprint Workflow

### Sprint Planning (Day 1)
1. Review product backlog
2. Select stories for the sprint
3. Define sprint goal
4. Break down stories into tasks
5. Create story folders and documentation

### Daily Work
1. Pick a story to work on
2. Update story status in sprint planning doc
3. Work through tasks
4. Document decisions in technical notes
5. Mark tasks complete as you go

### Sprint Review (Last Day)
1. Demo completed work
2. Update sprint planning doc with outcomes
3. Archive sprint folder
4. Document lessons learned

### Sprint Retrospective
Reflect on:
- What went well?
- What could be improved?
- Action items for next sprint

Document in `sprint-XX/retrospective.md`

## Story Workflow

### Story States
- **TODO** - Not started
- **IN PROGRESS** - Actively working
- **BLOCKED** - Waiting on something
- **IN REVIEW** - Code complete, testing
- **DONE** - Meets definition of done

### Working on a Story

1. **Start**: Update status to IN PROGRESS
2. **Implement**: Follow tasks in story README
3. **Document**: Add notes to technical-notes.md
4. **Test**: Complete testing.md scenarios
5. **Review**: Check against acceptance criteria
6. **Complete**: Update status to DONE

## Definition of Done

See [definition-of-done.md](definition-of-done.md) for detailed criteria.

Quick checklist:
- [ ] Code implemented and tested
- [ ] Documentation updated
- [ ] No breaking changes (or documented migration)
- [ ] Committed to repository

## Story Template

Use the template in [story-template.md](story-template.md) when creating new stories.

## Tips for Solo Development

- **Keep stories small**: 1-3 days of work max
- **Document as you go**: Future you will thank you
- **Be realistic**: Don't overcommit in sprint planning
- **Stay flexible**: Adjust sprint scope if needed
- **Celebrate wins**: Mark stories DONE and move forward

## Tools

- **Documentation**: Markdown files in this folder structure
- **Version Control**: Git commits tied to story numbers
- **Task Tracking**: Checkboxes in story README.md files

## Sprint Naming Convention

- `sprint-01`, `sprint-02`, etc.
- Each sprint folder contains:
  - `sprint-planning.md` - Goals and story list
  - `story-XX-name/` - Individual story folders
  - `retrospective.md` - End of sprint reflection

## Story Naming Convention

Format: `story-XX-short-description`

Examples:
- `story-01-replace-worlds-with-pitches`
- `story-02-remove-regions`
- `story-03-admin-prompt-config-database`

Keep folder names lowercase with hyphens for consistency.

