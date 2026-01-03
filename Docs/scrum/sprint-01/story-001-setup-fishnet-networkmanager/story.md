# Story 001: Setup FishNet NetworkManager Scene

## Story Information

- **Story ID**: 001
- **Priority**: High
- **Sprint**: Sprint 01
- **Status**: In Progress
- **Created**: 2026-01-03
- **Started**: 2026-01-03
- **Completed**: -

## User Story

**As a** developer  
**I want** a properly configured FishNet NetworkManager scene  
**So that** I have the foundation for all multiplayer functionality

## Background / Context

FishNet has been imported into the project but nothing has been configured yet. We need the NetworkManager set up as the base for all networking features.

## Acceptance Criteria

- [ ] **AC1**: NetworkManager GameObject exists in a dedicated network scene
- [ ] **AC2**: FishNet NetworkManager component is configured
- [ ] **AC3**: Transport layer is set up (Tugboat recommended for simple TCP/UDP)
- [ ] **AC4**: Can start as Host (server + client)
- [ ] **AC5**: Can start as Client and connect to a Host
- [ ] **AC6**: Basic connection/disconnection logs appear in console

## Tasks

- [ ] Create a new "NetworkManager" scene
- [ ] Add NetworkManager GameObject with FishNet components
- [ ] Configure Tugboat transport
- [ ] Add temporary UI buttons for Host/Client/Stop
- [ ] Test local connection (Host on one instance, Client on another)
- [ ] Verify connection events fire correctly

## Technical Notes

- **Approach**: Minimal setup - just get connection working
- **Technologies**: FishNet, Tugboat Transport
- **Dependencies**: None
- **Risks**: None expected for basic setup

## Questions / Decisions

### Open Questions
- [ ] Should NetworkManager be in its own scene or added to existing scenes?
- [ ] What port should we use for connections?

### Decisions Made
- *(To be filled as we work)*
