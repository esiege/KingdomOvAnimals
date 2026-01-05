# EncounterController

Main orchestrator for the game session. Manages turn flow, player coordination, and game state.

## Responsibilities

- Initialize game (deck shuffle, initial draw)
- Manage turn order and turn number
- Coordinate between Player and Opponent controllers
- Handle network game events
- Manage disconnect UI

## Scene Setup

Requires references to:
- `player` - PlayerController for local player
- `opponent` - PlayerController for opponent
- `playerHandController` - HandController for player
- `opponentHandController` - HandController for opponent
- `turnIndicatorText` - UI text for turn display
- `disconnectPanel` / `disconnectStatusText` - Disconnect UI

## Game Modes

### Single Player Mode
Detected when `NetworkGameManager.Instance` is null:
1. Shuffles both decks locally
2. Draws initial hands
3. Player always goes first

### Network Mode
Detected when NetworkGameManager exists:
1. Waits for `OnNetworkGameStarted()` callback
2. Uses synchronized shuffle seed
3. First player determined by server

## Turn Flow

### Turn Start (`StartTurn` / `StartTurnNetwork`)
1. Set `currentPlayer` reference
2. Increment max mana (turn 2+)
3. Refill mana to max
4. Reset board (clear tapped/summoning sickness)
5. Draw one card
6. Update playable card highlights

### Turn End (`EndTurn`)
1. Hide all targeting highlights
2. Switch `currentPlayer` to other player
3. Increment `turnNumber`
4. Start next player's turn

## Key Methods

| Method | Description |
|--------|-------------|
| `InitializeEncounter()` | Single-player initialization |
| `OnNetworkGameStarted()` | Network game initialization |
| `OnNetworkTurnChanged()` | Handle networked turn change |
| `StartTurn()` | Begin current player's turn |
| `EndTurn()` | End current turn, switch players |
| `DrawCard()` | Draw card for specified player |

## Network Callbacks

| Callback | Trigger |
|----------|---------|
| `OnNetworkGameStarted` | Both players ready, game begins |
| `OnNetworkTurnChanged` | Server changes turn |
| `OnOpponentReconnected` | Disconnected opponent returns |
| `OnOpponentForfeited` | Opponent grace period expired |
| `OnHostDisconnected` | Host connection lost (client only) |
| `OnHostReconnected` | Host returned (client only) |

## State Properties

| Property | Type | Description |
|----------|------|-------------|
| `currentPlayer` | PlayerController | Active player this turn |
| `turnNumber` | int | Current turn count |
| `isGamePaused` | bool | Game paused (disconnect) |
| `maxHandSize` | int | Maximum cards in hand (5) |

---
*Parent: [Controllers](./README.md)*
