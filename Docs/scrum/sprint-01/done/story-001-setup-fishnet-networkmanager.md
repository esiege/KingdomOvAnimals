# Story 001: Setup FishNet NetworkManager Scene

## Story Information

- **Story ID**: 001
- **Priority**: High
- **Sprint**: Sprint 01
- **Status**: ✅ Complete
- **Created**: 2026-01-03
- **Started**: 2026-01-03
- **Completed**: 2026-01-03

## User Story

**As a** developer  
**I want** a properly configured FishNet NetworkManager scene  
**So that** I have the foundation for all multiplayer functionality

## Background / Context

FishNet has been imported into the project but nothing has been configured yet. We need the NetworkManager set up as the base for all networking features.

## Acceptance Criteria

### AC1: Host can start a server
**Test:** Click the "Host" button
**Expected:** Console shows "Server connection state: Started" and "Client connection state: Started"

### AC2: Client can connect to Host
**Test:** With Host running on one instance, click "Client" on second instance
**Expected:** Console shows "Successfully connected to server!" on the client

### AC3: Connection can be stopped
**Test:** Click "Stop" button while connected
**Expected:** Console shows "Disconnected from server" and connections close cleanly

### AC4: Connection status is visible
**Test:** Perform any connection action
**Expected:** Status text on screen updates to reflect current state (Ready/Host/Client/Stopped)

### AC5: Two separate game instances can connect
**Test:** Run a build + Unity Editor (or two builds), one as Host, one as Client
**Expected:** Both instances show successful connection in console logs

---

## Tasks

- [x] Create ConnectionManager script
- [x] Create NetworkTestUI script
- [ ] In Unity: Create "NetworkTest" scene (or use existing)
- [ ] In Unity: Drag `FishNet/Demos/Prefabs/NetworkManager.prefab` into scene
- [ ] In Unity: Verify Tugboat transport is attached to NetworkManager
- [ ] In Unity: Create Canvas with Host/Client/Stop buttons
- [ ] In Unity: Add ConnectionManager to scene, attach NetworkTestUI
- [ ] Test: Build two instances, Host on one, Client on other
- [ ] Verify connection events fire correctly in console

## Implementation Notes

### Scripts Created
1. **ConnectionManager.cs** (`Assets/Scripts/Network/`)
   - Handles server/client start/stop
   - Subscribes to connection state events
   - Logs connection status changes

2. **NetworkTestUI.cs** (`Assets/Scripts/Network/`)
   - Simple UI for testing
   - Host/Client/Stop buttons
   - Status text display

### Unity Setup Steps
1. Open/Create the NetworkTest scene
2. Drag `Assets/FishNet/Demos/Prefabs/NetworkManager.prefab` into hierarchy
3. The NetworkManager prefab already has Tugboat transport configured
4. Create a Canvas with 3 buttons (Host, Client, Stop) and a status Text
5. Add empty GameObject, attach `ConnectionManager` script
6. Add `NetworkTestUI` to Canvas, wire up references

### Testing
1. Build the project
2. Run build as Host
3. Run Unity Editor as Client (or second build)
4. Check console for connection logs

## Questions / Decisions

### Open Questions
- [ ] Should NetworkManager be in its own scene or added to existing scenes?
- [ ] What port should we use for connections?

### Decisions Made
- *(To be filled as we work)*
