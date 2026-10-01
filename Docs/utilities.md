# Utilities

*Verified against source: 2026-08-12 (vdate)*

## ConsoleLogToFile

Captures Unity console output to a file. Attached to a persistent GameObject.

### Output Files

Editor and Build write to **different files** so host/client logs don't clobber each other:

| Environment | File |
|-------------|------|
| Editor | `Docs/log_editor.log` |
| Build | `Docs/log_build.log` |

(`.log` extension is intentional — Unity ignores it for domain reload, unlike `.txt`.)

### Features
- Timestamp on each line (configurable)
- Includes stack traces (configurable)
- Separate singleton instance per environment

---

## ReconnectionManager

Static class (`Assets/Scripts/Network/ReconnectionManager.cs`) that survives MonoBehaviour destruction (uses
Unity's `Application.quitting` / `EditorApplication.update` hooks rather than a scene lifecycle). Logs to
`Logs/GameLogs/reconnect_manager_editor.log` / `reconnect_manager_build.log`. As of this writing its
`StartReconnectionWait()` entry point has no call sites elsewhere in the codebase — see
[architecture.md](./architecture.md#network) for current status.

---

## CardLibrary

Singleton (`Assets/Scripts/Data/CardLibrary.cs`, `DontDestroyOnLoad`) that loads all `CardData` and
`AbilityData` ScriptableObjects from `Resources/Cards` and `Resources/Abilities` on `Awake` and indexes them by
ID.

### Methods
| Method | Description |
|--------|-------------|
| `GetCardById(id)` / `GetCardDataById(id)` | Look up a `CardData` (the two are aliases) |
| `GetAbilityDataById(id)` | Look up an `AbilityData` |
| `GetAllCardData()` / `GetAllAbilityData()` | Full lists (copies) |
| `GetCardDataByType(CardType)` | Filter by card type |
| `HasCardData(id)` / `HasAbilityData(id)` | Existence checks |
| `CardDataCount` / `AbilityDataCount` | Counts |
| `ReloadDataAssets()` | Re-scan Resources (editor use) |

`CardLibrary.EnsureInitialized()` creates the singleton on demand if it doesn't exist yet.

---

## Adding New Utilities

1. Create the script in `Assets/Scripts/Utilities/`
2. Document purpose and usage here
3. Prefer stateless helpers; use ScriptableObjects for data containers

---
*Back to [Main Documentation](./README.md)*
