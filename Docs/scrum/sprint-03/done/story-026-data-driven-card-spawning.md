# Story 026: Data-Driven Card Spawning

## Status: Done (superseded)
## Sprint: 03
## Dependencies: 022
## Started: 2026-01-07

> **Reclassified 2026-08-12 (vdate):** self-reported "In Progress" was stale. The goal (single prefab spawns
> from CardData, no per-card prefabs) shipped via Story 036: `Assets/Scripts/View/CardView.cs` is exactly this —
> one prefab, `Initialize(CardState, CardLibrary)` pulls all data from `CardData`.

---

## User Story

**As a** developer  
**I want** cards to spawn from a single prefab using CardData  
**So that** adding new cards only requires creating data assets, not prefabs

---

## Description

Migrate from individual card prefabs to a single generic Card prefab that gets populated from CardData at runtime.

**Current State:**
- PlayerController.deck = List<CardController> (prefab references)
- Each card type has its own prefab in scene
- DrawCard passes prefab reference to HandController

**Target State:**
- PlayerController.deck = List<CardData> (data references)
- One generic Card.prefab with empty CardData field
- DrawCard instantiates generic prefab, calls InitializeFromData(cardData)

---

## Acceptance Criteria

- [ ] I can see a single "Card" prefab in Prefabs/Card folder
- [ ] I can assign CardData assets to PlayerController.deck in Inspector
- [ ] When I draw a card, it displays the correct name from CardData
- [ ] When I draw a card, it displays the correct health from CardData
- [ ] When I draw a card, it displays the correct mana cost from CardData
- [ ] Different cards in my hand show different stats based on their CardData

---

## Testing Instructions

1. Open the Duel scene
2. Check PlayerController - deck should now accept CardData assets
3. Assign CardData assets (lion, bear, snake, etc.) to the deck
4. Enter Play mode
5. Draw cards - each should show stats from their CardData
6. Verify different cards have different names/stats

---

## Technical Tasks

1. [ ] Change PlayerController.deck from List<CardController> to List<CardData>
2. [ ] Add cardPrefab reference to EncounterController
3. [ ] Update DrawCard() to instantiate from prefab + InitializeFromData
4. [ ] Update HandController.AddCardToHand() to accept CardData
5. [ ] Remove individual card prefabs from scene decks
6. [ ] Test card drawing works with new system

---

## Definition of Done

- [ ] Code complete
- [ ] Cards spawn correctly from CardData
- [ ] No individual card prefabs needed in deck
- [ ] Acceptance criteria met
- [ ] Story closed in backlog

---

## Notes

- Keep Card.prefab generic - no CardData assigned by default
- Existing prefabs can remain for reference but shouldn't be in deck lists
- Network sync will need to be addressed in future story

