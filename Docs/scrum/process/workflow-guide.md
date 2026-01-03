# Workflow Guide

## Overview

This guide describes the day-to-day workflow for working with Scrum in this project, from sprint planning through story completion.

## Sprint Workflow

### Phase 1: Sprint Planning (Day 1)

**Duration**: 2-4 hours

**Goals**:
1. Define sprint goal
2. Select stories from backlog
3. Break down stories into tasks
4. Commit to sprint scope

**Steps**:

1. **Review Previous Sprint** (if applicable)
   - Read previous retrospective
   - Check for incomplete stories
   - Note any action items

2. **Define Sprint Goal**
   - Create `sprint-goals.md` in sprint folder
   - Write 1-2 sentence sprint goal
   - List 3-5 key outcomes expected

3. **Select Stories**
   - Review backlog (or create initial stories)
   - Prioritize by value and dependencies
   - Estimate story points
   - Select stories totaling reasonable points for 2 weeks
   - Recommended: 20-30 points for solo developer

4. **Create Story Documentation**
   - For each story, create folder: `story-XXX-name/`
   - Copy story template to `story.md`
   - Fill in user story, acceptance criteria
   - Break down into tasks
   - Estimate story points

5. **Create Sprint Plan**
   - Create `sprint-planning.md`
   - List all stories
   - Document dependencies
   - Note any risks or unknowns

**Outputs**:
- `sprint-XX/sprint-goals.md`
- `sprint-XX/sprint-planning.md`
- `sprint-XX/story-XXX-name/story.md` for each story

---

### Phase 2: Sprint Execution (Days 2-9)

**Daily Workflow**:

#### Morning: Plan the Day (15-30 min)

1. **Review Current Status**
   ```bash
   # Check what's in progress
   # Review yesterday's progress notes
   # Check for any blockers
   ```

2. **Select Today's Work**
   - Choose 1-2 tasks from current story
   - Or select next story if current is done
   - Be realistic about time available

3. **Update Story Status**
   - Update story.md with status: "In Progress"
   - Mark current tasks as in progress

#### During the Day: Execute Work

1. **Focus on One Story**
   - Complete one story before starting another (when possible)
   - Work through tasks sequentially

2. **Document as You Go**
   - Add notes to `technical-notes.md`
   - Document decisions
   - Track any issues or blockers

3. **Commit Regularly**
   - Make small, focused commits
   - Write clear commit messages
   - Reference story number: "Story-003: Add prompt table migration"

#### Evening: Wrap Up (15-30 min)

1. **Update Progress**
   - Check off completed tasks
   - Add progress notes to story.md
   - Update story status if changed

2. **Document Blockers**
   - Note any blockers in story.md
   - Document questions or decisions needed

3. **Plan Tomorrow**
   - Identify next tasks to work on
   - Note any preparation needed

---

### Phase 3: Sprint Review (Day 10 Morning)

**Duration**: 1-2 hours

**Goals**:
1. Review completed work
2. Verify acceptance criteria
3. Demo functionality
4. Document sprint results

**Steps**:

1. **Review Each Story**
   - Go through each story's acceptance criteria
   - Verify all are met
   - Test functionality
   - Mark story as "Done" or document why not

2. **Create Completion Documentation**
   - For completed stories, create `completed.md`
   - Document lessons learned
   - Note any follow-up items

3. **Update Sprint Planning**
   - Mark stories as complete or incomplete
   - Document velocity (story points completed)
   - Note any stories moved to next sprint

4. **Demo to Yourself** (or stakeholders if applicable)
   - Run through completed features
   - Verify they work end-to-end
   - Take screenshots or recordings if useful

**Outputs**:
- Updated `sprint-planning.md` with results
- `completed.md` for each finished story
- List of incomplete stories for backlog

---

### Phase 4: Sprint Retrospective (Day 10 Afternoon)

**Duration**: 1 hour

**Goals**:
1. Reflect on what went well
2. Identify improvements
3. Create action items
4. Learn and adapt

**Steps**:

1. **Create Retrospective Document**
   - Create `sprint-XX/retrospective.md`
   - Use retrospective template

2. **Reflect on Sprint**
   - What went well?
   - What didn't go well?
   - What was learned?
   - What should change?

3. **Identify Action Items**
   - Process improvements
   - Technical improvements
   - Documentation updates
   - Training or learning needed

4. **Update Process Documents**
   - Update DoD if needed
   - Update templates if needed
   - Document new patterns or practices

**Outputs**:
- `sprint-XX/retrospective.md`
- Action items for next sprint
- Updated process documentation

---

## Story Workflow Details

### Story States

```
Backlog → Planned → In Progress → In Review → Done
```

**Backlog**: Story created but not scheduled
**Planned**: Story selected for current sprint
**In Progress**: Development started
**In Review**: Code complete, testing/reviewing
**Done**: Meets Definition of Done

### Working on a Story

#### 1. Start Story
- [ ] Move story to "In Progress"
- [ ] Read story.md completely
- [ ] Review acceptance criteria
- [ ] Understand technical approach
- [ ] Note start date

#### 2. During Development
- [ ] Work through tasks sequentially
- [ ] Check off tasks as completed
- [ ] Document decisions in technical-notes.md
- [ ] Write tests as you go
- [ ] Commit code regularly
- [ ] Update progress notes daily

#### 3. Code Complete
- [ ] All tasks checked off
- [ ] All acceptance criteria met
- [ ] Tests written and passing
- [ ] Code cleaned up
- [ ] Move to "In Review"

#### 4. Self-Review
- [ ] Complete DoD checklist
- [ ] Run all tests
- [ ] Test manually
- [ ] Review own code
- [ ] Check documentation
- [ ] Fix any issues found

#### 5. Complete Story
- [ ] All DoD criteria met
- [ ] Create `completed.md`
- [ ] Mark story as "Done"
- [ ] Note completion date
- [ ] Commit final changes

### Story Folder Organization

```
story-XXX-name/
├── story.md                 # Main story doc (always)
├── technical-notes.md       # Implementation details (created when starting)
├── testing-notes.md         # Test cases (created when testing)
├── completed.md             # Final summary (created when done)
└── attachments/             # Screenshots, diagrams (optional)
```

---

## Working with Dependencies

### Story Depends on Another Story

1. **Document Dependency**
   - Note in "Dependencies" section
   - Link to dependent story

2. **Plan Accordingly**
   - Ensure dependent story is done first
   - Or plan to work on dependent story next

3. **Block if Needed**
   - Mark story as blocked
   - Document blocker in progress notes
   - Switch to unblocked work

### Discovering New Dependencies

1. **Document in Story**
   - Add to dependencies section
   - Note when discovered

2. **Update Sprint Plan**
   - May need to re-sequence stories
   - May need to push story to next sprint

3. **Create New Story if Needed**
   - If dependency is significant, create story
   - Add to backlog or current sprint

---

## Handling Blockers

### When Blocked

1. **Document Blocker**
   - Add to story progress notes
   - Describe what's blocking
   - Note when blocked

2. **Try to Unblock**
   - Research solutions
   - Ask questions (to AI, forums, docs)
   - Document attempts

3. **Switch Work if Needed**
   - Move to different story
   - Mark blocked story status
   - Return when unblocked

4. **Escalate if Critical**
   - Document in sprint planning
   - May affect sprint goal
   - Consider scope adjustment

---

## Best Practices

### Time Management

- **Timeboxing**: Limit research/debugging to 2-4 hours before changing approach
- **Focus Time**: Block 2-4 hour chunks for deep work
- **Break Tasks Down**: If task takes > 4 hours, break it down further
- **Buffer Time**: Don't commit to 100% of available time

### Documentation

- **Document Decisions**: Write down why, not just what
- **Keep Notes Current**: Update documentation daily
- **Be Honest**: Document struggles and failures, not just successes
- **Future Self**: Write for someone (including future you) who knows nothing

### Code Quality

- **Test First**: Write tests before or alongside code
- **Refactor Continuously**: Clean up as you go
- **Review Often**: Self-review before moving to next task
- **Commit Frequently**: Small, focused commits with clear messages

### Sprint Health

- **Check Velocity**: Are you completing planned work?
- **Watch for Overcommitment**: Better to under-commit and exceed
- **Adjust Scope**: It's OK to move stories to next sprint
- **Learn from Pain**: Use retrospectives to improve

---

## Common Scenarios

### Scenario: Story Takes Longer Than Expected

1. Update story estimate
2. Document why (learning, complexity, blockers)
3. Consider splitting story
4. May need to move other stories to next sprint
5. Discuss in retrospective

### Scenario: Story is Too Large

1. Stop work
2. Split into multiple stories
3. Complete smallest viable part
4. Move rest to backlog/next sprint
5. Update dependencies

### Scenario: Found a Bug While Working

1. Document bug in progress notes
2. If critical: Fix immediately
3. If not critical: Create bug story for backlog
4. Continue with current story

### Scenario: Discovered Technical Debt

1. Document in technical-notes.md
2. Create technical debt story for backlog
3. Continue with current story
4. Address debt in future sprint

### Scenario: Ran Out of Time in Sprint

1. Document in sprint planning what's incomplete
2. Move incomplete stories to next sprint or backlog
3. Discuss in retrospective
4. Adjust planning for next sprint

---

## Tools and Commands

### Creating a New Story

```bash
# Navigate to sprint folder
cd docs/scrum/sprint-XX

# Create story folder
mkdir story-XXX-brief-name

# Copy template
cp ../process/story-template.md story-XXX-brief-name/story.md

# Edit story
code story-XXX-brief-name/story.md
```

### Checking Sprint Progress

```bash
# View all stories in current sprint
ls docs/scrum/sprint-XX/story-*/

# Check which stories are done
grep -r "Status.*Done" docs/scrum/sprint-XX/story-*/story.md

# Count completed tasks
grep -r "\[x\]" docs/scrum/sprint-XX/story-*/story.md | wc -l
```

### Git Workflow

```bash
# Create feature branch for story
git checkout -b story-XXX-brief-description

# Commit with story reference
git commit -m "Story-XXX: Description of change"

# Merge back to main when done
git checkout main
git merge story-XXX-brief-description
git branch -d story-XXX-brief-description
```

---

## Next Steps

1. **Read**: [Story Template](story-template.md)
2. **Review**: [Definition of Done](definition-of-done.md)
3. **Start**: Sprint 01 Planning
4. **Execute**: Follow this workflow guide

## Questions?

Document questions in your story's progress notes or in sprint retrospectives for continuous improvement.
