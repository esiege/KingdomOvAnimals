# Utilities

Helper classes and services used across the project.

## ConsoleLogToFile

Debug utility that writes Unity console logs to a text file for post-session analysis.

### Purpose
- Capture log output from builds where console isn't visible
- Debug reconnection issues across multiple sessions
- Track timing of events with timestamps

### Usage
Attached to a GameObject in scene. Automatically captures `Debug.Log`, `Debug.LogWarning`, and `Debug.LogError`.

### Output Files
| File | Location |
|------|----------|
| Editor | `Assets/../reconnect_editor.txt` |
| Build | `<build_folder>/reconnect_build.txt` |

### Features
- Timestamp on each log entry
- Separates editor vs build logs
- Appends to existing file (doesn't overwrite)
- Flushes after each write

### Configuration
| Property | Description |
|----------|-------------|
| `logFileName` | Base filename for log output |
| `includeTimestamp` | Add timestamp prefix |
| `logLevel` | Filter by log severity |

---

## CardLibrary

ScriptableObject asset containing all card templates in the game.

### Purpose
- Central registry of all CardTemplates
- Easy lookup by card ID or name
- Used for deck building and spawning cards

### Methods

| Method | Description |
|--------|-------------|
| `GetCardById(string id)` | Find template by unique ID |
| `GetCardByName(string name)` | Find template by display name |
| `GetAllCards()` | Return complete card list |
| `GetCardsByType(CardType type)` | Filter by creature/spell type |

### Location
`Assets/Resources/CardLibrary` - Must be in Resources for runtime loading.

### Network Usage
- Server selects cards by ID
- ID transmitted over network
- Client looks up local template

---

## Adding New Utilities

1. Create script in `Assets/Scripts/Utilities/`
2. Document purpose and usage in this file
3. Keep utilities stateless where possible
4. Prefer ScriptableObjects for data containers

---
*Back to [Main Documentation](./README.md)*
