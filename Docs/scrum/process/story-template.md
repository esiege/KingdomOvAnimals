# Story Template

Use this template when creating a new user story. Copy this into each story folder as `story.md`.

---

# Story [XXX]: [Brief Title]

## Story Information

- **Story ID**: XXX
- **Story Points**: [1, 2, 3, 5, 8, 13]
- **Priority**: [High, Medium, Low]
- **Sprint**: Sprint XX
- **Status**: [Backlog, Planned, In Progress, In Review, Done]
- **Assigned**: [Your name or unassigned]
- **Created**: YYYY-MM-DD
- **Started**: YYYY-MM-DD
- **Completed**: YYYY-MM-DD

## User Story

**As a** [type of user]  
**I want** [goal/desire]  
**So that** [benefit/value]

### Example
**As a** game master  
**I want** to manage LLM prompts through an admin interface  
**So that** I can fine-tune content generation without modifying code

## Background / Context

[Provide context about why this story is needed. What problem does it solve? What's the current situation?]

## Acceptance Criteria

Define what "done" means for this story. Be specific and testable.

- [ ] **AC1**: [Specific, measurable criterion]
- [ ] **AC2**: [Specific, measurable criterion]
- [ ] **AC3**: [Specific, measurable criterion]
- [ ] **AC4**: [Specific, measurable criterion]

### Example Acceptance Criteria
- [ ] Admin can view a list of all prompt templates in the system
- [ ] Admin can edit a prompt template and save changes
- [ ] Changes to prompts are persisted in the database
- [ ] Updated prompts are used immediately in content generation
- [ ] System validates prompt template format before saving

## Tasks

Break the story down into concrete, actionable tasks. Check off as completed.

### Database Changes
- [ ] Task 1: [Specific task description]
- [ ] Task 2: [Specific task description]

### Backend Implementation
- [ ] Task 3: [Specific task description]
- [ ] Task 4: [Specific task description]

### Frontend Implementation (if applicable)
- [ ] Task 5: [Specific task description]
- [ ] Task 6: [Specific task description]

### Testing
- [ ] Task 7: Write unit tests
- [ ] Task 8: Write integration tests
- [ ] Task 9: Manual testing

### Documentation
- [ ] Task 10: Update API documentation
- [ ] Task 11: Update user documentation

## Technical Notes

[High-level technical approach. Detailed implementation notes should go in technical-notes.md]

- **Approach**: [Describe the general technical approach]
- **Technologies**: [List relevant technologies, libraries, frameworks]
- **Dependencies**: [List any dependencies on other stories or external systems]
- **Risks**: [Identify potential technical risks or challenges]

## Dependencies

- **Depends On**: [List story IDs this story depends on]
- **Blocks**: [List story IDs that depend on this story]
- **Related**: [List related stories]

## Questions / Decisions

Track open questions and decisions made during development.

### Open Questions
- [ ] Question 1: [What needs to be decided?]
- [ ] Question 2: [What's unclear?]

### Decisions Made
- **Decision 1**: [What was decided and why]
  - Date: YYYY-MM-DD
  - Rationale: [Why this decision was made]

## Testing Strategy

[Brief overview of testing approach. Details in testing-notes.md]

- **Unit Tests**: [What will be unit tested]
- **Integration Tests**: [What will be integration tested]
- **Manual Tests**: [What requires manual testing]

## Progress Notes

Track progress and blockers as you work.

### YYYY-MM-DD
- [Progress update]
- [Blockers or challenges]

### YYYY-MM-DD
- [Progress update]

## Definition of Done Checklist

Before marking this story as complete, verify:

- [ ] All acceptance criteria met
- [ ] All tasks completed
- [ ] Code reviewed (self-review checklist)
- [ ] Tests written and passing
- [ ] Documentation updated
- [ ] No critical bugs
- [ ] Code merged to main branch
- [ ] Ready for deployment/release

## Lessons Learned

[After completion, document what you learned. Move detailed notes to completed.md]

### What Went Well
- [Success 1]

### What Could Be Improved
- [Improvement 1]

### Key Takeaways
- [Takeaway 1]

---

## Related Documents

- [Technical Notes](technical-notes.md) - Detailed implementation notes
- [Testing Notes](testing-notes.md) - Test cases and testing details
- [Completed Summary](completed.md) - Final summary (created when done)
