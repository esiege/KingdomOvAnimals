# Story 005: Sync Card Plays Across Network

## Story Information

- **Story ID**: 005
- **Priority**: High
- **Sprint**: Sprint 01
- **Status**: ✅ Complete
- **Created**: 2026-01-03
- **Started**: 2026-01-03
- **Completed**: 2026-01-04

## User Story

**As a** player  
**I want** to see cards my opponent plays appear on their side of the board  
**So that** I can see the game state and respond to their moves

## Background / Context

Currently, `HandController.addCardToEncounter()` handles playing cards locally:
1. Validates mana cost and slot availability
2. Spends mana via `PlayerController.SpendMana()`
3. Moves card GameObject to slot
4. Updates card state (removes from hand, enters play)
5. Adds to `PlayerController.board` list

For multiplayer, when Player A plays a card:
- Player A's client should send a request to the server
- Server validates the play (mana, slot, turn)
- Server spawns a networked card on the target slot
- Both clients see the card appear

## Acceptance Criteria

### AC1: Card play request sent to server
**Test:** Player drags card to slot during their turn
**Expected:** Server receives play request with card ID and slot index

### AC2: Server validates card play
**Test:** Player attempts to play card without enough mana
**Expected:** Server rejects the play, card stays in hand

### AC3: Card appears on both screens
**Test:** Player successfully plays a card
**Expected:** Card appears in correct slot on both players' screens

### AC4: Mana deducted on play
**Test:** Player plays a 2-mana card with 3 mana
**Expected:** Both players see mana go from 3 to 1

---

## Tasks

- [ ] Create ServerRpc for card play requests
- [ ] Server validates play (mana, slot, turn)
- [ ] Server spawns networked card prefab
- [ ] Sync card position/slot to all clients
- [ ] Update hand state on card play
- [ ] Test: Play card as host, verify client sees it
- [ ] Test: Play card as client, verify host sees it

---

## Technical Notes

### Current Flow (Local Only)
```
HandController.addCardToEncounter(card, hitObject)
├── Validate: !card.isInPlay
├── Validate: owningPlayer == currentPlayer  
├── Validate: slot not occupied
├── Validate: enough mana
├── currentPlayer.SpendMana(card.manaCost)
├── Move card to slot (transform.SetParent, position)
├── Update card state (isInHand=false, summoningSickness, etc.)
├── HandController.RemoveCardFromHand(card.id)
└── currentPlayer.AddCardToBoard(card)
```

### Network Flow (Proposed)
```
Client: HandController.addCardToEncounter()
├── Get card data (cardName/prefabName, slotIndex)
├── NetworkPlayer.CmdPlayCard(cardName, slotIndex)
│
Server: NetworkPlayer.CmdPlayCard()
├── Validate: IsOwner's turn
├── Validate: Enough mana
├── Validate: Slot available
├── Spawn networked card prefab
├── Position in slot
├── Deduct mana (SyncVar auto-syncs)
└── Card appears on all clients via spawn

Client Callback: OnCardSpawned
├── Update local hand state
└── Update UI
```

### Key Decisions
1. **Card Prefabs**: Need networked card prefabs registered with FishNet
2. **Card Identity**: Use prefab name to identify which card to spawn
3. **Slot Indexing**: PlayerSlot-1, PlayerSlot-2, PlayerSlot-3 → indices 0, 1, 2
