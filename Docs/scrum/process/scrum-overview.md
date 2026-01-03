# Scrum Process Overview

## What is Scrum?

Scrum is an agile framework for managing work with an emphasis on software development. It's designed for teams of three to nine members who break their work into actions that can be completed within time-boxed iterations called sprints (typically 2 weeks).

## Scrum Framework for This Project

Since this is a solo/small team project, we're adapting traditional Scrum practices to fit the context while maintaining the benefits of structured planning and iteration.

## Core Scrum Elements

### 1. Sprint
- **Duration**: 2 weeks (10 working days)
- **Goal**: Each sprint has a clear, measurable goal
- **Outcome**: Potentially shippable product increment

### 2. User Stories
- **Format**: As a [user type], I want [goal] so that [reason]
- **Size**: Should be completable within 1-5 days
- **Components**: 
  - User story statement
  - Acceptance criteria
  - Tasks (technical breakdown)
  - Story points (complexity estimate)

### 3. Story Points
Using Fibonacci sequence for estimation:
- **1 point**: Very simple, well-understood task (1-2 hours)
- **2 points**: Simple task with minimal complexity (2-4 hours)
- **3 points**: Moderate task, some unknowns (4-8 hours)
- **5 points**: Complex task, multiple components (1-2 days)
- **8 points**: Very complex, needs breakdown (2-3 days)
- **13 points**: Too large, should be split into multiple stories

### 4. Sprint Ceremonies (Adapted)

#### Sprint Planning (Day 1)
- Review sprint goal
- Select stories for sprint
- Break down stories into tasks
- Commit to sprint backlog
- **Output**: sprint-planning.md

#### Daily Standup (Daily - Self Check-in)
- What did I complete yesterday?
- What will I work on today?
- Any blockers or questions?
- **Output**: Update story progress in documentation

#### Sprint Review (Last Day)
- Demo completed work
- Review acceptance criteria
- Gather feedback (if applicable)
- **Output**: Update sprint-planning.md with results

#### Sprint Retrospective (Last Day)
- What went well?
- What could be improved?
- Action items for next sprint
- **Output**: Create retrospective.md

## Story Workflow

```
1. BACKLOG → Story created in backlog
2. PLANNED → Story selected for sprint
3. IN PROGRESS → Development started
4. IN REVIEW → Code review / Testing
5. DONE → Meets definition of done
```

## Definition of Ready (Story can enter sprint)

A story is ready when it has:
- [ ] Clear user story statement
- [ ] Defined acceptance criteria
- [ ] Story point estimate
- [ ] No blockers or dependencies resolved
- [ ] Technical approach understood

## Definition of Done (Story is complete)

See [definition-of-done.md](definition-of-done.md) for complete checklist.

## Documentation Standards

### File Naming
- Use kebab-case: `my-file-name.md`
- Be descriptive but concise
- Include dates where relevant: `retrospective-2026-01-14.md`

### Folder Structure
```
sprint-XX/
├── sprint-planning.md
├── sprint-goals.md
├── retrospective.md (created at end)
└── story-XXX-name/
    ├── story.md (main documentation)
    ├── technical-notes.md
    ├── testing-notes.md
    └── completed.md (created when done)
```

## Best Practices

1. **Keep Stories Small**: If a story feels too big, break it down
2. **Update Regularly**: Keep documentation current as you work
3. **Write Clear Acceptance Criteria**: Be specific about what "done" means
4. **Technical Debt**: Track it as stories, don't ignore it
5. **Learn and Adapt**: Use retrospectives to improve process

## Tools for Solo Development

- **Documentation**: Markdown files in this repository
- **Task Tracking**: Update story.md files with task status
- **Version Control**: Git branches per story (optional)
- **Code Review**: Self-review checklist before marking done

## Adapting Traditional Scrum

**Traditional Scrum** | **Our Adaptation**
--- | ---
Product Owner | You (prioritize based on value)
Scrum Master | You (facilitate process)
Development Team | You (implement)
Daily Standup Meeting | Written check-in / status update
Sprint Review with Stakeholders | Self-review against acceptance criteria
Retrospective Discussion | Written reflection

## Getting Started

1. Read the [Story Template](story-template.md)
2. Review the [Definition of Done](definition-of-done.md)
3. Check the [Workflow Guide](workflow-guide.md)
4. Start with Sprint Planning for Sprint 01
