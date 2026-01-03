# Definition of Done

## Overview

The Definition of Done (DoD) is a checklist that ensures every user story meets consistent quality standards before being marked as complete. This prevents technical debt and ensures reliable, maintainable code.

## General Definition of Done

A user story is considered "Done" when ALL of the following criteria are met:

### 1. Functionality
- [ ] All acceptance criteria are met
- [ ] All tasks in the story are completed
- [ ] Feature works as described in the user story
- [ ] Edge cases and error scenarios are handled

### 2. Code Quality
- [ ] Code follows project conventions and style guidelines
- [ ] Code is well-structured and maintainable
- [ ] No commented-out code or debug statements
- [ ] No compiler warnings introduced
- [ ] Complex logic is explained with comments
- [ ] Code is DRY (Don't Repeat Yourself)

### 3. Testing
- [ ] Unit tests written for new functionality
- [ ] Integration tests written where applicable
- [ ] All tests pass (including existing tests)
- [ ] Test coverage is adequate (aim for >80% of new code)
- [ ] Manual testing performed for user-facing features
- [ ] Tested in relevant environments (local, dev)

### 4. Code Review
- [ ] Self-review completed using checklist
- [ ] Code reviewed by another developer (if applicable)
- [ ] All review comments addressed
- [ ] Code refactored based on feedback

### 5. Documentation
- [ ] Code is self-documenting (clear naming, structure)
- [ ] API documentation updated (if applicable)
- [ ] User-facing documentation updated (if applicable)
- [ ] Technical documentation updated (architecture, data models)
- [ ] README updated (if needed)
- [ ] Comments added for complex logic

### 6. Database Changes
- [ ] Migration scripts created and tested
- [ ] Migration documented in development/migrations.md
- [ ] Rollback strategy documented
- [ ] No breaking changes to existing data
- [ ] Database schema documentation updated

### 7. Integration
- [ ] Code compiles without errors
- [ ] Code builds successfully
- [ ] All dependencies resolved
- [ ] No merge conflicts
- [ ] Integrated with main branch
- [ ] Version control best practices followed

### 8. Deployment Readiness
- [ ] Configuration changes documented
- [ ] Environment variables identified and documented
- [ ] No hardcoded values (API keys, connection strings)
- [ ] Deployment steps documented (if new)
- [ ] Backward compatible (or breaking changes documented)

### 9. Performance
- [ ] No obvious performance issues introduced
- [ ] Database queries optimized
- [ ] Large datasets handled efficiently
- [ ] No memory leaks

### 10. Security
- [ ] No security vulnerabilities introduced
- [ ] Authentication/authorization working correctly
- [ ] Sensitive data properly protected
- [ ] Input validation implemented
- [ ] SQL injection prevented (parameterized queries)

## Story-Specific Criteria

### For Database Schema Changes
- [ ] Entity Framework migration created
- [ ] Migration tested locally
- [ ] Seed data updated (if needed)
- [ ] Migration documented
- [ ] DbContext updated
- [ ] Repository/Helper classes updated

### For API Endpoints
- [ ] HTTP methods are RESTful
- [ ] Request/response models defined
- [ ] Error responses standardized
- [ ] Authentication implemented
- [ ] Authorization implemented
- [ ] Input validation implemented
- [ ] API tested with Postman/curl
- [ ] API documentation updated

### For LLM Integration
- [ ] Prompts tested with multiple providers
- [ ] Prompt templates validated
- [ ] Response parsing tested
- [ ] Error handling for API failures
- [ ] Rate limiting considered
- [ ] Cost implications documented
- [ ] Token usage optimized

### For Frontend Changes
- [ ] UI components functional
- [ ] Responsive design implemented
- [ ] Accessibility standards met
- [ ] Browser compatibility verified
- [ ] Loading states implemented
- [ ] Error states implemented
- [ ] User feedback provided (messages, notifications)

### For Admin Features
- [ ] Only accessible to admins
- [ ] Input validation comprehensive
- [ ] Audit trail implemented (if needed)
- [ ] Cannot break the system through admin actions
- [ ] Backup/restore considered

## Self-Review Checklist

Before marking a story as done, complete this self-review:

### Code Review Questions
1. Would this code make sense to someone reading it for the first time?
2. Are variable and function names clear and descriptive?
3. Is the code structured logically?
4. Have I removed all debugging code and console logs?
5. Are there any obvious bugs or edge cases I missed?
6. Could this code be simplified?
7. Is error handling comprehensive?
8. Are there any potential security issues?
9. Would this code work correctly with unexpected input?
10. Is this code testable?

### Testing Review Questions
1. Have I tested the happy path?
2. Have I tested error scenarios?
3. Have I tested edge cases?
4. Have I tested with realistic data?
5. Do all tests pass?
6. Are the tests maintainable and clear?

### Documentation Review Questions
1. Is the purpose of this code clear?
2. Are complex algorithms explained?
3. Is the API documentation accurate?
4. Would a new developer understand how to use this?

## When to Skip Criteria

Some DoD items may not apply to every story. Document any skipped items and rationale:

- **Unit Tests**: May skip for simple configuration changes
- **Integration Tests**: May skip for UI-only changes
- **Frontend Changes**: N/A for backend-only stories
- **Database Changes**: N/A for stories without schema changes

**Important**: Document why any criteria are skipped in the story's completed.md file.

## Continuous Improvement

This Definition of Done should evolve. After each sprint retrospective, consider:
- Are these criteria still relevant?
- Do we need additional criteria?
- Are any criteria too strict or too lenient?
- What issues could have been prevented with better DoD?

Update this document as the project and team learn and grow.

## Enforcement

- No story is marked "Done" without meeting DoD
- Technical debt stories should be created for intentional DoD violations
- Regular reviews ensure DoD compliance
- DoD violations are learning opportunities, not failures
