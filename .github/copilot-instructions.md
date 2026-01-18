# GitHub Copilot Instructions for KingdomOvAnimals

## Project Overview
KingdomOvAnimals is a multiplayer card game built with Unity and FishNet networking.

## Code Style & Architecture

### Always Use Tools
- **Always put Editor scripts in `Assets/Scripts/Editor/`** - Unity requires this for Editor-only code
- **Always use namespaces** - Use `KOA.Model`, `KOA.View`, `KOA.Network`, `KOA.Data`, `KOA.Logic`, `KOA.Migration`, `KOA.Editor`
- **Always add XML documentation** - Public classes and methods should have `<summary>` comments

### Board State Architecture (Story 030)
- **Players are integers**: Player 0 and Player 1 (never "Player"/"Opponent" strings)
- **Slots are integers**: Slot 0, 1, 2 (never "PlayerSlot-1" magic strings)
- **Single source of truth**: `BoardState` owns game state, `NetworkBoardState` syncs it
- **View layer has no logic**: `CardView`, `BoardView` only render, no game rules
- **Perspective only in view**: `BoardView.GetSlotTransform()` handles visual mirroring

### FishNet Networking
- Use `[ServerRpc]` for client-to-server commands (prefix with `Cmd`)
- Use `[ObserversRpc]` for server-to-all-clients broadcasts (prefix with `Rpc`)
- Use `[TargetRpc]` for server-to-specific-client (prefix with `Target`)
- Custom serializers needed for non-primitive SyncVar types

### Unity Version Compatibility
- Use `FindObjectOfType<T>()` instead of `FindFirstObjectByType<T>()` (Unity 2021 compatibility)
- Use `FindObjectsOfType<T>()` instead of `FindObjectsByType<T>()` (Unity 2021 compatibility)

### CardData Properties
- `id` - Unique identifier string
- `displayName` - Human-readable name
- `health` - Card health points
- `manaCost` - Mana cost to play
- `artwork` - Card image sprite
- `offensiveAbility` - AbilityData for attacks
- `defensiveAbility` - AbilityData for defense/support

### When Deprecating or Refactoring Code
- **Always update editor scripts** - When deprecating classes or changing architecture, check `Assets/Scripts/Editor/` for scripts that reference the old code
- **Use BoardStateSetupWindow** - The canonical setup tool is at `KOA → Board State Setup` - update it when adding new required components
- **Check PlayerSpawner** - FishNet's `PlayerSpawner` needs `NetworkPlayer` prefab assigned, or players won't spawn
- **Scene setup matters** - Many runtime errors come from missing scene setup (prefab assignments, component references)

## File Locations
- **Data (ScriptableObjects)**: `Assets/Scripts/Data/`
- **Model (pure data classes)**: `Assets/Scripts/Model/`
- **View (MonoBehaviour visuals)**: `Assets/Scripts/View/`
- **Network (FishNet sync)**: `Assets/Scripts/Network/`
- **Logic (pure C# helpers)**: `Assets/Scripts/Logic/`
- **Editor (editor-only)**: `Assets/Scripts/Editor/`
- **Migration (old↔new bridges)**: `Assets/Scripts/Migration/`
- **Documentation**: `Docs/scrum/stories/`

## Testing
- Use the Editor window: **KOA → Board State Setup** for setup validation
- Test with two clients (host + client build) for multiplayer verification
