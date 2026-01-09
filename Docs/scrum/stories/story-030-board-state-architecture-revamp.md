# Story 030: Board State Architecture Revamp

## User Story
**As a** developer  
**I want** a clean data-driven board state architecture  
**So that** multiplayer synchronization is reliable and code is maintainable

## Problem Statement

### Current Issues
The current implementation has accumulated technical debt making multiplayer card interactions buggy and hard to debug:

1. **Magic strings everywhere**
   - `"PlayerSlot-1"`, `"OpponentSlot-2"` - string parsing for slot identification
   - `"Player"`, `"Opponent"` - string comparison for perspective detection
   - Typos cause silent failures, no compile-time safety

2. **Perspective baked into identifiers**
   - "PlayerSlot" vs "OpponentSlot" is relative to viewer
   - Same physical slot has different names on different clients
   - Requires constant translation/mirroring logic

3. **Multiple sources of truth**
   - Cards parented to slot GameObjects (scene hierarchy)
   - Cards in `PlayerController.cardsOnBoard` list
   - Cards in `HandController` hand list
   - NetworkPlayer syncs health/mana but not board state
   - Easy for these to get out of sync

4. **Mixed responsibilities**
   - `NetworkPlayer` (1420 lines): network sync, card tracking, targeting, UI updates, RPCs, slot resolution
   - `PlayerController`: UI display, health/mana, board tracking (duplicates NetworkPlayer!)
   - `HandController`: hand management, input handling, card play, targeting visualization

5. **Fragile slot lookup**
   - `GameObject.Find("SlotName")` scattered throughout
   - Fallback `FlipSlotPerspective` finds wrong cards
   - No validation that slots exist

6. **Complex perspective translation**
   - `ResolveSlotNameForPlayer()` - 50+ lines of perspective logic
   - `TranslateClientSlotToServer()` - more translation
   - `FlipSlotPerspective()` - yet more translation
   - Mirroring logic in `RpcExecuteCardPlay`
   - Each function has subtle differences causing bugs

### Root Cause
**No canonical data model.** Game state is implicit in scene hierarchy rather than explicit in code.

---

## Proposed Architecture

### Design Principles

1. **Integers not strings** - Players are `0` and `1`. Slots are `0`, `1`, `2`. No parsing.
2. **Single source of truth** - `GameState` holds all authoritative data
3. **Server authoritative** - Server owns game state, clients render it
4. **View is dumb** - Presentation layer only renders, doesn't track state
5. **No perspective in data** - Data is always `players[playerId].board[slotIndex]`
6. **Explicit over implicit** - Board slots are arrays, not scene hierarchy

### Layer Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         MODEL (Data Layer)                          │
├─────────────────────────────────────────────────────────────────────┤
│  BoardState                                                         │
│  ├── Players[2]: PlayerState                                        │
│  │   └── PlayerState                                                │
│  │       ├── PlayerId: int                                          │
│  │       ├── Health: int                                            │
│  │       ├── Mana: int                                              │
│  │       ├── Hand: List<CardState>                                  │
│  │       ├── Board: CardState[3]  (null = empty slot)               │
│  │       └── Deck: List<CardState>                                  │
│  ├── CurrentTurnPlayerId: int                                       │
│  └── TurnNumber: int                                                │
│                                                                     │
│  CardState (lightweight data, not MonoBehaviour)                    │
│  ├── CardDataId: string (references CardData ScriptableObject)      │
│  ├── InstanceId: int (unique per card instance)                     │
│  ├── CurrentHealth: int                                             │
│  ├── IsTapped: bool                                                 │
│  ├── HasSummoningSickness: bool                                     │
│  └── OwnerId: int                                                   │
└─────────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      NETWORK (Sync Layer)                           │
├─────────────────────────────────────────────────────────────────────┤
│  NetworkBoardState : NetworkBehaviour                               │
│  ├── SyncVar<BoardState> or SyncList approach                       │
│  ├── Server authoritative - only server modifies                    │
│  ├── Clients receive updates via OnChange callbacks                 │
│  └── RPCs use universal identifiers:                                │
│      - PlayCard(playerId, handIndex, slotIndex)                     │
│      - UseAbility(attackerPlayerId, attackerSlot,                   │
│                   targetPlayerId, targetSlot)                       │
│      - DrawCard(playerId)                                           │
│                                                                     │
│  NetworkPlayer : NetworkBehaviour (simplified)                      │
│  ├── PlayerId: SyncVar<int>                                         │
│  ├── Connection ownership                                           │
│  └── Input relay to NetworkBoardState                               │
└─────────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       VIEW (Presentation Layer)                     │
├─────────────────────────────────────────────────────────────────────┤
│  BoardView : MonoBehaviour                                          │
│  ├── LocalPlayerId: int (which player am I?)                        │
│  ├── SlotViews[2][3]: Transform (indexed by playerId, slotIndex)    │
│  │   - MySlots = SlotViews[LocalPlayerId]                           │
│  │   - TheirSlots = SlotViews[1 - LocalPlayerId]                    │
│  ├── CardViews: Dictionary<int, CardView> (instanceId -> view)      │
│  └── RenderBoard(BoardState) - positions all cards                  │
│                                                                     │
│  CardView : MonoBehaviour                                           │
│  ├── Visual representation of a card                                │
│  ├── Health/mana text, card image, status icons                     │
│  └── UpdateFromState(CardState)                                     │
│                                                                     │
│  HandView : MonoBehaviour                                           │
│  ├── Renders hand cards                                             │
│  └── Handles visual positioning only                                │
│                                                                     │
│  PlayerStatsView : MonoBehaviour                                    │
│  ├── Health/mana display                                            │
│  └── UpdateFromState(PlayerState)                                   │
└─────────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     CONTROLLER (Input Layer)                        │
├─────────────────────────────────────────────────────────────────────┤
│  InputController : MonoBehaviour                                    │
│  ├── Handles mouse/touch input                                      │
│  ├── Raycast to detect card/slot clicks                             │
│  ├── Drag and drop state machine                                    │
│  └── Sends commands to NetworkBoardState:                           │
│      - RequestPlayCard(handIndex, slotIndex)                        │
│      - RequestUseAbility(mySlot, targetPlayerId, targetSlot)        │
│      - RequestEndTurn()                                             │
│                                                                     │
│  TargetingHelper                                                    │
│  ├── Pure functions, no state                                       │
│  ├── GetValidPlaySlots(BoardState, playerId) -> int[]               │
│  ├── GetValidAbilityTargets(BoardState, attackerId, slot) -> List   │
│  └── CanPlayCard(BoardState, playerId, handIndex) -> bool           │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Design Decisions

### Decision 1: Board slots as arrays vs scene hierarchy

**Options:**
- A) Keep cards parented to slot GameObjects
- B) Track cards in `CardState[3]` array, position visually

**Decision: B - Array-based tracking**

**Rationale:**
- Arrays are indexable by integer, no string lookup
- Clear ownership: `boardState.Players[0].Board[1]` is unambiguous
- Easier to sync over network (serialize array vs scene hierarchy)
- Testing: can unit test game logic without scene

---

### Decision 2: CardState as data vs CardController MonoBehaviour

**Options:**
- A) Keep `CardController` MonoBehaviour as source of truth
- B) Separate `CardState` (data) from `CardView` (visual)

**Decision: B - Separate data and view**

**Rationale:**
- CardState can be serialized for network sync
- CardState can exist without GameObject (deck, destroyed cards)
- Enables replays, AI simulation, unit testing
- CardView is just a renderer, can be pooled

---

### Decision 3: Where does board state live?

**Options:**
- A) In `NetworkPlayer` (current approach, per-player)
- B) In new `NetworkBoardState` singleton
- C) In `NetworkGameManager`

**Decision: B - New NetworkBoardState singleton**

**Rationale:**
- Board state is game-level, not player-level
- Single sync point is easier to debug
- NetworkPlayer becomes simpler (just identity + connection)
- Matches mental model: "the game" has state, players interact with it

---

### Decision 4: How to identify targets in RPCs

**Options:**
- A) Slot names like `"OpponentSlot-2"` (current)
- B) Tuple `(playerId, slotIndex)` 
- C) Card instance ID

**Decision: B - (playerId, slotIndex) tuples**

**Rationale:**
- Integers are unambiguous across all clients
- No perspective translation needed
- Validates naturally: `playerId ∈ {0,1}`, `slotIndex ∈ {0,1,2}`
- Instance IDs are overkill when slots are fixed

---

### Decision 5: How does view know which side to render?

**Options:**
- A) "Player" vs "Opponent" naming (current)
- B) `LocalPlayerId` property, view calculates

**Decision: B - LocalPlayerId-based rendering**

**Rationale:**
- Data says `players[0].board[1]` - always consistent
- View knows `LocalPlayerId = 1`, so renders `players[0]` on opponent side
- Simple conditional: `isMyCard = (state.OwnerId == LocalPlayerId)`
- No "perspective" in data layer at all

---

### Decision 6: Slot mirroring for visual layout

**Options:**
- A) Mirror in data layer (current mess)
- B) Mirror only in view layer
- C) Don't mirror - accept different visual layout

**Decision: B - Mirror only in view layer**

**Rationale:**
- Front row for me should appear as back row for opponent (visual only)
- Data always says `slot 0, 1, 2` - no mirroring
- `BoardView.GetSlotPosition(playerId, slotIndex)` handles visual placement
- One place for all mirroring logic, isolated from game logic

---

### Decision 7: What to do with existing controllers?

**Options:**
- A) Delete and rewrite
- B) Refactor incrementally
- C) Wrap with adapter layer

**Decision: B - Refactor incrementally**

**Rationale:**
- Less risk than full rewrite
- Can test at each step
- Preserve working input/targeting logic where possible
- Goal: migrate state tracking, keep UI wiring

---

### Decision 8: SyncVar vs SyncList for board state

**Options:**
- A) SyncVar<BoardState> (serialize whole state)
- B) SyncList<CardState> per player
- C) Individual SyncVars for each field

**Decision: B - SyncList per player with targeted RPCs**

**Rationale:**
- SyncList handles add/remove/change efficiently
- Don't re-sync entire state on every change
- RPCs for actions, SyncList for state verification
- Matches FishNet patterns

---

## Implementation Plan

### Phase 1: Data Model (No behavior changes)
1. Create `CardState` class (plain C# data)
2. Create `PlayerBoardState` class with `CardState[3] Board`
3. Create `BoardState` class aggregating players
4. Add serialization support for FishNet

**Acceptance:** Classes compile, can instantiate, serialize

### Phase 2: NetworkBoardState (Parallel to existing)
1. Create `NetworkBoardState : NetworkBehaviour`
2. Add SyncLists for each player's board
3. Implement server-side state modification methods
4. Add RPC stubs (no implementation yet)

**Acceptance:** Can spawn NetworkBoardState, see synced empty boards

### Phase 3: View Layer (New, alongside existing)
1. Create `BoardView` with slot transforms setup
2. Create `CardView` prefab and pool
3. Implement `RenderBoard(BoardState)` 
4. Subscribe to NetworkBoardState changes

**Acceptance:** Empty board renders correctly for both players

### Phase 4: Card Play Flow (First integration)
1. Wire input to `NetworkBoardState.RequestPlayCard()`
2. Implement server-side card play logic
3. Update SyncList, notify clients
4. BoardView creates/positions CardView

**Acceptance:** Can play card, appears on both clients in correct position

### Phase 5: Ability Targeting (Core combat)
1. Implement `RequestUseAbility(attackerSlot, targetPlayerId, targetSlot)`
2. Server validates and applies damage
3. Update CardState.CurrentHealth in SyncList
4. CardView updates health display
5. Handle card death (remove from board)

**Acceptance:** Can attack cards, damage syncs, death removes card

### Phase 6: Migration & Cleanup
1. Remove old slot resolution code from NetworkPlayer
2. Remove string-based slot lookups
3. Slim down NetworkPlayer to identity only
4. Update EncounterController to use new system
5. Remove redundant PlayerController.cardsOnBoard tracking

**Acceptance:** All old perspective code removed, tests pass

### Phase 7: Polish & Edge Cases
1. Hand management integration
2. Turn system integration
3. Card draw integration
4. Summoning sickness, tap states
5. Support abilities (healing, buffs)
6. Player targeting (direct damage)

**Acceptance:** Full game loop works with new architecture

---

## File Structure

```
Assets/Scripts/
├── Model/
│   ├── CardState.cs
│   ├── PlayerBoardState.cs
│   └── BoardState.cs
├── Network/
│   ├── NetworkBoardState.cs      (NEW - main sync)
│   ├── NetworkPlayer.cs          (SIMPLIFIED)
│   └── NetworkGameManager.cs     (minimal changes)
├── View/
│   ├── BoardView.cs              (NEW)
│   ├── CardView.cs               (NEW - replaces CardController visuals)
│   ├── HandView.cs               (NEW or refactored)
│   └── PlayerStatsView.cs        (NEW or refactored)
├── Controllers/
│   ├── InputController.cs        (NEW or refactored from HandController)
│   ├── TargetingHelper.cs        (NEW - pure functions)
│   ├── CardController.cs         (DEPRECATED - migrate to CardView)
│   ├── PlayerController.cs       (DEPRECATED - migrate to PlayerStatsView)
│   └── HandController.cs         (DEPRECATED - migrate to HandView + InputController)
└── Data/
    └── CardData.cs               (existing ScriptableObject)
```

---

## Acceptance Criteria

### Functional
- [ ] Two players can play cards to board slots
- [ ] Both players see cards in correct positions (visual mirroring works)
- [ ] Flip abilities target correct cards
- [ ] Damage applies to correct cards on both clients
- [ ] Dead cards removed from both clients
- [ ] Turn system works
- [ ] Hand management works
- [ ] No magic strings in network code

### Technical
- [ ] All slot references use integer indices
- [ ] All RPCs use `(playerId, slotIndex)` format
- [ ] BoardState is single source of truth
- [ ] View layer has no game logic
- [ ] NetworkPlayer under 200 lines
- [ ] No `GameObject.Find()` for slot lookup
- [ ] No "Player"/"Opponent" string comparisons

### Quality
- [ ] Code compiles with no warnings
- [ ] Existing tests pass (if any)
- [ ] Manual test: full game with both clients works
- [ ] Debug logging shows clean integer-based targeting

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Large refactor breaks everything | High | Incremental phases, test after each |
| SyncList complexity | Medium | Start simple, add optimization later |
| Visual bugs in mirroring | Medium | Isolate to BoardView, easy to fix |
| Input system breaks | Medium | Keep HandController working until Phase 6 |
| Reconnection breaks | High | Test reconnection at Phase 4 |

---

## Estimation

| Phase | Effort | 
|-------|--------|
| Phase 1: Data Model | 1 hour |
| Phase 2: NetworkBoardState | 2 hours |
| Phase 3: View Layer | 2 hours |
| Phase 4: Card Play | 2 hours |
| Phase 5: Ability Targeting | 2 hours |
| Phase 6: Migration | 2 hours |
| Phase 7: Polish | 2 hours |
| **Total** | **~13 hours** |

---

## Open Questions

1. **Keep CardController or fully replace?** 
   - Could keep CardController as CardView, just remove state tracking
   - Or create new CardView for cleaner break

2. **How to handle reconnection?**
   - NetworkBoardState needs to re-sync full state on reconnect
   - May need special reconnection RPC

3. **Card pooling?**
   - Worth implementing card object pool for CardViews?
   - Probably overkill for 6-slot board

4. **Animation/VFX integration?**
   - How do card movement animations work with new system?
   - CardView handles own animations based on state changes?

---

## References

- [FishNet SyncList Documentation](https://fish-networking.gitbook.io/docs/manual/guides/synchronizing/synclists)
- [Story 012: ScriptableObject Cards](./story-012-scriptableobject-cards.md)
- [Current Architecture](../../architecture.md)
