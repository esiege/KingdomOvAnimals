# Kingdom Ov Animals - Scrum Documentation

## Overview

This folder contains all Scrum artifacts for the Kingdom Ov Animals project, including sprint planning, user stories, and process documentation.

## Structure

```
scrum/
├── README.md (this file)
├── process/ (Scrum methodology documentation)
│   ├── scrum-overview.md
│   ├── story-template.md
│   ├── definition-of-done.md
│   └── workflow-guide.md
└── sprint-01/ (Current Sprint)
    ├── sprint-planning.md
    ├── sprint-goals.md
    └── story-XXX-name/
```

## Current Sprint

**Sprint 01** - Foundation & Core Gameplay Polish
- Start Date: January 3, 2026
- End Date: January 17, 2026 (2 weeks)
- Goal: Solidify core card gameplay mechanics and targeting system

## Quick Links

- [Scrum Process Guide](process/scrum-overview.md)
- [Sprint 01 Planning](sprint-01/sprint-planning.md)
- [Story Template](process/story-template.md)
- [Definition of Done](process/definition-of-done.md)

## Sprint History

| Sprint | Dates | Goal | Status |
|--------|-------|------|--------|
| Sprint 01 | Jan 3-17, 2026 | Foundation & Core Gameplay Polish | 🟡 In Progress |

## How to Use This Documentation

1. **Starting a New Sprint**: Copy the sprint template and create a new sprint-XX folder
2. **Creating Stories**: Use the story template in `process/story-template.md`
3. **Working on Stories**: Each story folder contains all artifacts for that story
4. **Review Process**: Follow the workflow-guide.md for daily standup, reviews, and retrospectives

## Key Concepts

### Story Folder Contents
Each story folder should contain:
- `story.md` - Main story documentation (user story, acceptance criteria, tasks)
- `technical-notes.md` - Implementation details and decisions
- `testing-notes.md` - Test cases and testing approach
- `completed.md` - Final summary and lessons learned (created upon completion)

### Story Naming Convention
`story-XXX-brief-descriptive-name/`
- XXX = Sequential number (001, 002, etc.)
- brief-descriptive-name = Kebab-case description (max 4-5 words)

### Sprint Naming Convention
`sprint-XX/` where XX is the sprint number (01, 02, 03, etc.)
