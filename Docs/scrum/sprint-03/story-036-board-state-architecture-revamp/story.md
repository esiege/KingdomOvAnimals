# Story 036: Board State Architecture Revamp

## Status: 🟡 IN PROGRESS (Phase 4 Code Complete, Needs Testing)

**Last Updated:** January 17, 2026

| Phase | Status | Description |
|-------|--------|-------------|
| Phase 1: Data Model | ✅ Complete | CardState, PlayerBoardState, BoardState with FishNet serializers |
| Phase 2: NetworkBoardState | ✅ Complete | Server-authoritative sync with SyncVar<BoardState> |
| Phase 3: View Layer | ✅ Complete | BoardView, CardView, InputController - no game logic in views |
| Phase 4: Card Play Flow | 🟡 Code Complete | CmdPlayCard fixed, InitializeGame wired up, needs scene setup & testing |
| Phase 5: Ability Targeting | ❌ TODO | Combat through NetworkBoardState |
| Phase 6: Delete Old Code | ❌ TODO | Remove deprecated NetworkPlayer/HandController methods |
| Phase 7: Polish | ❌ TODO | Full game loop integration |

---

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

**Decision: A - Delete and rewrite (REPLACEMENT)**

**Rationale:**
- Old system is fundamentally broken (magic strings, multiple sources of truth)
- "Migrating" leads to two parallel systems and confusion
- BoardState IS the source of truth - old controllers are DEPRECATED
- Clean break prevents half-measures and bridge code
- Old CardController/HandController/PlayerController will be REMOVED, not adapted

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

### Phase 1: Data Model (No behavior changes) ✅ COMPLETE
1. Create `CardState` class (plain C# data)
2. Create `PlayerBoardState` class with `CardState[3] Board`
3. Create `BoardState` class aggregating players
4. Add serialization support for FishNet

**Acceptance:** Classes compile, can instantiate, serialize

**Outcome (Jan 17, 2026):**
- `Assets/Scripts/Model/CardState.cs` - Pure data class with FishNet serializer, `InstanceId`, `CardDataId`, `OwnerId`, status flags
- `Assets/Scripts/Model/PlayerBoardState.cs` - `Board[3]`, `Hand`, `Deck`, `Health`, `Mana`, `DrawCard()`, `PlaceCard()`, `RemoveHandCard()`
- `Assets/Scripts/Model/BoardState.cs` - `Players[2]`, `CurrentTurnPlayerId`, `TurnNumber`, `FindCardOnBoard()`, FishNet serializer
- All use integer IDs (no magic strings), XML documented

### Phase 2: NetworkBoardState (Parallel to existing) ✅ COMPLETE
1. Create `NetworkBoardState : NetworkBehaviour`
2. Add SyncLists for each player's board
3. Implement server-side state modification methods
4. Add RPC stubs (no implementation yet)

**Acceptance:** Can spawn NetworkBoardState, see synced empty boards

**Outcome (Jan 17, 2026):**
- `Assets/Scripts/Network/NetworkBoardState.cs` (649 lines) - Singleton with `SyncVar<BoardState>`
- `InitializeGame(deck0, deck1)` - Shuffles decks, draws initial hands
- `CmdPlayCard(playerId, handIndex, slotIndex)` - Server-authoritative card play with full validation
- `CmdUseBoardAbility`, `CmdUseFlipAbility`, `CmdAttackPlayer`, `CmdEndTurn` - All use integer IDs
- Events: `OnCardPlayed`, `OnCardDamaged`, `OnCardDied`, `OnTurnChanged`, `OnPlayerDamaged`, `OnGameEnded`
- RPCs broadcast with `(playerId, slotIndex)` tuples - no magic strings

### Phase 3: View Layer (New, alongside existing) ✅ COMPLETE
1. Create `BoardView` with slot transforms setup
2. Create `CardView` prefab and pool
3. Implement `RenderBoard(BoardState)` 
4. Subscribe to NetworkBoardState changes

**Acceptance:** Empty board renders correctly for both players

**Outcome (Jan 17, 2026):**
- `Assets/Scripts/View/BoardView.cs` (463 lines) - `RenderBoard()`, `GetSlotTransform()` with perspective mirroring, `FindAndSetLocalPlayerId()` from network
- `Assets/Scripts/View/CardView.cs` (215 lines) - Visual only, `UpdateFromState()`, no game logic
- `Assets/Scripts/View/InputController.cs` (397 lines) - Drag/drop, calls `NetworkBoardState.CmdPlayCard/CmdUseBoardAbility`
- `Assets/Scripts/View/HandView.cs`, `TurnUI.cs`, `PlayerStatsView.cs` - Supporting views
- `Assets/Scripts/Logic/TargetingHelper.cs` (211 lines) - Pure functions for valid target calculation

### Phase 4: Card Play Flow (REPLACEMENT) � CODE COMPLETE - NEEDS TESTING

**CRITICAL: BoardState is the ONLY source of truth. No bridging to old system.**

1. Card dealing populates `BoardState.Players[].Hand` directly
2. InputController reads from BoardState, sends commands to NetworkBoardState
3. NetworkBoardState validates against BoardState (its own data)
4. Server updates BoardState, broadcasts RPC with integer IDs
5. Clients update visuals from BoardState changes

**Changes Made (Jan 17, 2026):**

✅ **Fixed `NetworkBoardState.CmdPlayCard`** - Now self-contained:
- Changed signature from `(playerId, handIndex, slotIndex, cardDataId, manaCost)` to `(playerId, handIndex, slotIndex)`
- Server gets card from `BoardState.Players[playerId].Hand[handIndex]`
- Server looks up `CardData` via `_cardLibrary.GetCardById()`
- Validates: turn, mana, slot empty
- Executes: `RemoveHandCard()`, `PlaceCard()`, `SpendMana()`

✅ **Added `InitializeNetworkBoardState()` to `NetworkGameManager.ServerStartGame`**:
- Loads decks from `Resources/Decks`
- Converts `DeckData.cards` to list of card IDs
- Calls `NetworkBoardState.Instance.InitializeGame(player0Deck, player1Deck)`
- Hands are populated via `DrawCardInternal()` → `PlayerBoardState.DrawCard()`

✅ **Added `FindAndSetLocalPlayerId()` to `BoardView.Start`**:
- Finds `NetworkPlayer` with `IsOwner == true`
- Sets `LocalPlayerId` for correct perspective rendering

**Remaining Blockers:**
- Scene setup: `NetworkBoardState` must be added to scene with `CardLibrary` reference
- Prefab: `CardView` prefab must exist and be assigned to `BoardView._cardViewPrefab`
- Resources: `DeckData` assets must exist in `Resources/Decks`

**Acceptance:** Can play card, appears on both clients in correct position. Old NetworkPlayer.CmdPlayCard is NOT called.

### Phase 5: Ability Targeting (REPLACEMENT) ❌ TODO

**CRITICAL: All combat goes through NetworkBoardState with integer IDs. Old NetworkPlayer ability RPCs are DEAD CODE.**

1. HandController sends `NetworkBoardState.CmdUseBoardAbility(attackerPlayerId, attackerSlot, targetPlayerId, targetSlot)`
2. Server validates attacker/target exist in BoardState
3. Server calculates damage, updates CardState.CurrentHealth
4. Server determines death (health <= 0) - SERVER AUTHORITATIVE
5. Server broadcasts `RpcCardDamaged` and `RpcCardDied` with integer IDs
6. Clients update visuals based on RPC data, NOT local calculation

**Acceptance:** Can attack cards, damage syncs, death removes card on ALL clients. Old NetworkPlayer.CmdUseAbilityOnCard is NOT called.

### Phase 6: Delete Old Code ❌ TODO

**This phase REMOVES deprecated code. If Phase 4-5 are done correctly, this is just deletion.**

1. Delete NetworkPlayer card play/ability methods (CmdPlayCard, CmdUseAbilityOnCard, etc.)
2. Delete NetworkPlayer slot resolution functions (ResolveSlotNameForPlayer, TranslateClientSlotToServer, etc.)
3. Delete PlayerController.cardsOnBoard tracking
4. Delete HandController.playerHand (reads from BoardState instead)
5. NetworkPlayer becomes identity-only (~100 lines: PlayerId, PlayerName, connection)

**Acceptance:** NetworkPlayer under 150 lines. No magic strings in codebase. No GameObject.Find for slots.

### Phase 7: Polish & Edge Cases ❌ TODO
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

| Phase | Estimate | Actual | Status |
|-------|----------|--------|--------|
| Phase 1: Data Model | 1 hour | ~1 hour | ✅ Complete |
| Phase 2: NetworkBoardState | 2 hours | ~2 hours | ✅ Complete |
| Phase 3: View Layer | 2 hours | ~2 hours | ✅ Complete |
| Phase 4: Card Play | 2 hours | ~1.5 hours | 🟡 Code Complete |
| Phase 5: Ability Targeting | 2 hours | - | ❌ TODO |
| Phase 6: Delete Old Code | 2 hours | - | ❌ TODO |
| Phase 7: Polish | 2 hours | - | ❌ TODO |
| **Total** | **~13 hours** | **~6.5 hours** | **~50%** |

---

## Phase 4 Testing Checklist

Before Phase 4 can be marked complete, verify the following:

### Scene Setup
- [ ] `NetworkBoardState` GameObject exists in EncounterScene
- [ ] `NetworkBoardState` has `NetworkObject` component
- [ ] `NetworkBoardState._cardLibrary` is assigned to CardLibrary
- [ ] `BoardView` GameObject exists with slot transforms configured
- [ ] `BoardView._cardViewPrefab` is assigned
- [ ] `BoardView._cardLibrary` is assigned
- [ ] `InputController` exists with `_boardView` and `_cardLibrary` assigned

### Resources
- [ ] At least one `DeckData` asset exists in `Resources/Decks/`
- [ ] Deck has cards with valid `CardData.id` values
- [ ] `CardData` assets exist in `Resources/Cards/` matching deck card IDs

### Functional Test
1. [ ] Start as Host
2. [ ] Connect second client
3. [ ] Verify `ServerStartGame()` calls `InitializeNetworkBoardState()`
4. [ ] Verify console shows "Initializing NetworkBoardState" with deck info
5. [ ] Verify both clients show cards in hand (BoardView renders from BoardState.Hand)
6. [ ] Drag card from hand to empty slot
7. [ ] Verify card appears in slot on BOTH clients
8. [ ] Verify mana is deducted
9. [ ] Verify console shows integer-based logging: `player=0, slot=1` (no magic strings)

---

## Open Questions

1. **Keep CardController or fully replace?** 
   - ✅ RESOLVED: Created new `CardView` for cleaner break
   - Old `CardController` is deprecated, will be deleted in Phase 6

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

---

## Implementation Complete - Acceptance Criteria & Test Steps

### Files Created

**Model Layer** (`Assets/Scripts/Model/`):
- `CardState.cs` - Lightweight card instance data with FishNet serializer
- `PlayerBoardState.cs` - Player's board (3 slots), hand, deck, resources
- `BoardState.cs` - Complete game state for both players

**Network Layer** (`Assets/Scripts/Network/`):
- `NetworkBoardState.cs` - Server-authoritative synchronized state singleton

**View Layer** (`Assets/Scripts/View/`):
- `CardView.cs` - Visual representation of a card (no game logic)
- `BoardView.cs` - Board rendering with perspective handling
- `PlayerStatsView.cs` - Health/mana display
- `HandView.cs` - Hand card layout and hover effects
- `TurnUI.cs` - Turn indicator and end turn button
- `InputController.cs` - User input to game commands

**Logic Layer** (`Assets/Scripts/Logic/`):
- `TargetingHelper.cs` - Valid target calculation (pure logic, testable)

**Migration** (`Assets/Scripts/Migration/`):
- `BoardStateBridge.cs` - Bridge between old and new systems

### Acceptance Criteria Checklist

#### Architecture
- [x] `CardState` is pure data (no MonoBehaviour)
- [x] `CardView` is visual only (no game logic)
- [x] `BoardState` is single source of truth
- [x] All slot references use integer indices (0, 1, 2)
- [x] All player references use integer IDs (0, 1)
- [x] No "Player"/"Opponent" string comparisons in new code
- [x] No `GameObject.Find()` for slot lookup in new code
- [x] All RPCs use `(playerId, slotIndex)` format

#### Network Sync
- [x] `NetworkBoardState` uses `SyncVar<BoardState>`
- [x] Custom FishNet serializers for CardState, PlayerBoardState, BoardState
- [x] Server authoritative - all state changes go through server
- [x] Events for UI updates: OnCardPlayed, OnCardDamaged, OnCardDied, OnTurnChanged

#### View Layer
- [x] `BoardView` handles all visual perspective (mirroring only here)
- [x] `GetSlotTransform(playerId, slotIndex)` does perspective translation
- [x] `RenderBoard()` creates/updates/destroys CardViews from state
- [x] Opponent's cards appear in top slots, player's in bottom slots

### Test Steps

#### Step 1: Add NetworkBoardState to Scene
1. Open EncounterScene
2. Create empty GameObject named "NetworkBoardState"
3. Add `NetworkBoardState` component
4. Add `NetworkObject` component (for FishNet)
5. Assign CardLibrary reference
6. Save scene

#### Step 2: Add BoardView to Scene
1. Create empty GameObject named "BoardView"
2. Add `BoardView` component
3. Set up slot transforms (create or reuse existing slot GameObjects)
4. Create CardView prefab and assign
5. Save scene

#### Step 3: Add InputController
1. Add `InputController` component to BoardView (or new GO)
2. Assign BoardView reference
3. Assign CardLibrary reference
4. Set layer masks for card and slot detection

#### Step 4: Build and Test
1. Build the project (verify no compile errors)
2. Start host (Server + Client)
3. Connect second client
4. Verify both clients can see empty board

#### Step 5: Test Card Play (New System)
1. With new InputController, drag card from hand to board slot
2. Verify card appears in correct slot on BOTH clients
3. Verify slot index is same on both (no mirroring confusion)
4. Verify mana is deducted

#### Step 6: Test Board Ability
1. End turn so played card loses summoning sickness
2. Drag card on board toward enemy card
3. Verify damage is applied on BOTH clients
4. Verify card taps after using ability

#### Step 7: Test Turn System
1. Verify end turn button works
2. Verify cards untap at turn start
3. Verify mana increases at turn start
4. Verify turn indicator shows correct player

#### Step 8: Verify No Desync
1. Play several rounds
2. Verify card positions match on both clients
3. Verify health/mana match on both clients
4. Verify no console errors about mismatched state

### Known Limitations (Future Work)
1. **Card prefab needed** - Need to create CardView prefab with proper UI elements
2. **Deck/draw not fully wired** - DrawCard exists in model but needs turn-start integration
3. **VFX/Animations placeholder** - PlayDamageAnimation and PlayDeathAnimation are stubs
4. **Direct player attack** - Not yet implemented (EnemyPlayer target type)

### Implementation Rules

**DO:**
- BoardState is the ONLY source of truth for game state
- All commands go through NetworkBoardState with integer IDs (playerId, slotIndex)
- Server validates against BoardState, server determines outcomes (damage, death)
- Clients render based on RPC broadcasts, never calculate game logic locally

**DO NOT:**
- Call old NetworkPlayer card methods (they are deprecated)
- Use magic strings ("PlayerSlot-1", "OpponentSlot-2")
- Calculate card death on client (server authoritative)
- Create "bridge" or "sync" code between old and new systems
- Keep old system "working" alongside new system
