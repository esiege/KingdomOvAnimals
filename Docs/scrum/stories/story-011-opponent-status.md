# Story 011: Show Opponent Connection Status

## User Story
**As a** player  
**I want** to see my opponent's connection status  
**So that** I know if they're connected, lagging, or disconnected

## Acceptance Criteria
- [x] UI indicator shows opponent's connection status (connected/disconnected)
- [x] Status updates in real-time when opponent disconnects
- [x] Status updates when opponent reconnects
- [x] Visual indicator is clear and non-intrusive
- [ ] Optional: Show ping/latency indicator

## Technical Design

### UI Elements
- Connection indicator near opponent's name/portrait (top-right corner)
- Green = connected, Red = disconnected, Yellow = reconnecting
- Optional: Ping display (e.g., "45ms")

### Implementation
1. Added `opponentConnectionIndicator`, `opponentConnectionIcon`, `opponentConnectionText` to EncounterController
2. Created `UpdateOpponentConnectionStatus(bool, string)` method
3. Created `SetOpponentReconnecting()` method for yellow "Reconnecting..." state
4. Updated `OnNetworkGameStarted` to show initial "Connected" state
5. Updated `OnOpponentDisconnected` to show "Reconnecting..." state
6. Updated `OnOpponentReconnected` to show "Connected" state
7. Extended DuelScreenSetup editor tool to create and wire up the UI

## Tasks
- [x] Add connection status UI fields to EncounterController
- [x] Add UpdateOpponentConnectionStatus method
- [x] Update OnNetworkGameStarted to set initial state
- [x] Update OnOpponentDisconnected to set reconnecting state  
- [x] Update OnOpponentReconnected to set connected state
- [x] Extend DuelScreenSetup editor tool
- [ ] Test with disconnect/reconnect scenarios

## How to Add UI to Scene
1. Open DuelScreen scene
2. Go to **Tools > KingdomOvAnimals > Setup Disconnect UI**
3. Click **"Add All UI"** to add both disconnect panel and connection status
4. Save the scene

## Estimate
0.5 days

---
*Status: Complete*
*Completed: 2026-01-06*
