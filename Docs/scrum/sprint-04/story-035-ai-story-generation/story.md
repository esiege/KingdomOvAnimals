# Story 035: AI Story Generation Integration

## Story Information

- **Story ID**: 035
- **Story Points**: 8
- **Priority**: Low
- **Sprint**: Sprint 04
- **Status**: Planned
- **Assigned**: Unassigned
- **Created**: 2026-01-17

## User Story

**As a** player  
**I want** unique, contextual story branches generated for my adventure  
**So that** each playthrough feels fresh and responds to my choices

## Background / Context

With 107 animal classes and branching paths, hand-crafting all content is impractical. AI generation enables:
- Contextual narratives based on selected class
- References to previous choices and deck composition
- Infinite variety while maintaining coherence
- Dynamic difficulty and thematic consistency

This story establishes the foundation for AI-assisted content generation.

## Acceptance Criteria

- [ ] **AC1**: AIStoryGenerator service interface defined
- [ ] **AC2**: Prompt templates created for story branch generation
- [ ] **AC3**: Context injection includes: className, deck contents, previous choices, battle outcomes
- [ ] **AC4**: Generated branches conform to StoryBranchData structure
- [ ] **AC5**: Fallback to template branches if AI unavailable
- [ ] **AC6**: Content filtering/validation before presenting to player
- [ ] **AC7**: Generation occurs during loading screens (async)
- [ ] **AC8**: Rate limiting and error handling for API calls

## Tasks

### Service Interface
- [ ] Create IAIStoryGenerator interface
- [ ] Create StoryGenerationRequest (context data)
- [ ] Create StoryGenerationResponse (branch data)
- [ ] Create mock implementation for testing

### Prompt Engineering
- [ ] Create base prompt template for story branches
- [ ] Create prompt for specialization choices
- [ ] Create prompt for ally selection
- [ ] Create prompt for post-battle narratives
- [ ] Test prompts with various contexts

### Context Building
- [ ] Create StoryContext class to collect all relevant data
- [ ] Include animal class info
- [ ] Include current deck composition
- [ ] Include previous story choices
- [ ] Include battle history (wins/losses)

### API Integration
- [ ] Choose AI provider (OpenAI, Claude, etc.)
- [ ] Implement API client
- [ ] Handle authentication securely
- [ ] Implement retry logic and timeouts

### Validation
- [ ] Parse generated JSON to StoryBranchData
- [ ] Validate card references exist in library
- [ ] Filter inappropriate content
- [ ] Check narrative coherence (basic heuristics)

### Fallback System
- [ ] Detect generation failures
- [ ] Select appropriate template branch
- [ ] Log failures for analysis

## Technical Notes

- **Location**: `Assets/Scripts/Logic/AI/AIStoryGenerator.cs`
- **Prompts**: `Assets/Resources/Data/AI/PromptTemplates/`
- **Namespace**: `KOA.Logic.AI`

### Example Prompt Template
```
You are a fantasy storyteller for a card game. Generate a story branch for:

Animal Class: {className} ({scientificName})
Current Deck: {deckSummary}
Previous Choice: {lastChoiceText}
Battle Outcome: {lastBattleResult}

Create a scenario with 3 choices. Each choice should:
1. Advance the narrative naturally
2. Reward the player with cards from the {className} family
3. Lead to interesting consequences

Output JSON matching this schema:
{schemaExample}
```

### Security Considerations
- API keys stored securely (not in source)
- Rate limiting to prevent abuse
- Content filtering for generated text

## Dependencies

- **Depends On**: 029 (Story Branch Data), 027 (Animal Classification), 015 (CardLibrary)
- **Blocks**: None (additive feature)
- **Related**: All story-related stories

## Questions / Decisions

### Open Questions
- [ ] Which AI provider to use?
- [ ] On-device vs cloud generation?
- [ ] How to handle API costs?
- [ ] Caching strategy for generated content?

### Decisions Made
- **Fallback-first approach**: System works with templates, AI enhances
  - Date: 2026-01-17
  - Rationale: Ensures game is playable even without AI connectivity

## Definition of Done Checklist

- [ ] All acceptance criteria met
- [ ] All tasks completed
- [ ] Code reviewed
- [ ] Prompts tested with multiple contexts
- [ ] Fallback system verified
- [ ] No API key leaks
- [ ] Rate limiting works
