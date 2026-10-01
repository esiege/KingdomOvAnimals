# Card Data Manager

> **Partially implemented, simpler than described below.** *(checked 2026-08-12, vdate)* The actual scene is
> named **`CardManagement`** (`Assets/Scenes/CardManagement.unity`), not `DataManager`, and its scripts live in
> `Assets/Scripts/UI/CardManagement/` (`CardManagementController`, `CardEditorUI`, `CardListItem`,
> `AbilityEditorUI`, `AbilityListItem`, `DeckEditorUI`) — not `Assets/Scripts/DataManager/`. The rest of this
> page (Balance View, CSV import/export, mana-curve charts) is design vision layered on top of that simpler,
> real editor UI — verify against the scripts above before assuming a feature described below exists.

A dedicated Unity scene for managing card and ability data through a visual interface.

## Overview

Instead of hunting through ScriptableObject assets in folders, the **Card Data Manager** provides a spreadsheet-like interface for:
- Creating and editing cards
- Creating and editing abilities
- Building decks
- Balancing stats
- Previewing cards

## Scene: DataManager

```
Assets/Scenes/DataManager.unity
```

This is an **editor/tools scene**, not a gameplay scene. It runs in Play mode to provide full UI interactivity.

---

## UI Layout

```
┌─────────────────────────────────────────────────────────────────────────┐
│  Card Data Manager                               [Save All] [Export CSV]│
├─────────────────────────────────────────────────────────────────────────┤
│ [Cards] [Abilities] [Decks] [Balance View]                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────┐  ┌──────────────────────────────────────────────┐ │
│  │ Card List       │  │ Card Editor                                  │ │
│  │ ───────────────│  │ ──────────────────────────────────────────── │ │
│  │ [Search: ____] │  │                                              │ │
│  │                 │  │  Name: [Wolf________________]                │ │
│  │ > Wolf       ✓ │  │  ID:   wolf_001 (auto)                       │ │
│  │   Bear         │  │                                              │ │
│  │   Eagle        │  │  ┌─────────┐  Mana Cost: [2]                 │ │
│  │   Snake        │  │  │  Card   │  Health:    [4]                 │ │
│  │   Spider       │  │  │  Image  │  Rarity:    [Common ▼]          │ │
│  │   Dragon       │  │  │ Preview │  Tribe:     [Beast  ▼]          │ │
│  │                 │  │  └─────────┘                                 │ │
│  │                 │  │                                              │ │
│  │ [+ New Card]   │  │  Offensive Ability:                          │ │
│  │                 │  │  [Bite - 3 Damage ▼▼▼▼▼▼▼▼▼▼▼▼▼] [Edit]     │ │
│  │                 │  │                                              │ │
│  │                 │  │  Defensive Ability:                          │ │
│  │                 │  │  [Howl - Buff +1 ▼▼▼▼▼▼▼▼▼▼▼▼▼] [Edit]      │ │
│  │                 │  │                                              │ │
│  │                 │  │  Description:                                │ │
│  │                 │  │  [A fierce wolf that attacks in packs.     ]│ │
│  │                 │  │  [___________________________________________]│ │
│  │                 │  │                                              │ │
│  │                 │  │  [Save Card] [Delete] [Duplicate]            │ │
│  └─────────────────┘  └──────────────────────────────────────────────┘ │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Tabs

### Cards Tab
- List all CardData assets
- Search/filter by name, tribe, rarity
- Create, edit, delete cards
- Live preview of card appearance

### Abilities Tab
```
┌─────────────────┐  ┌──────────────────────────────────────────────┐
│ Ability List    │  │ Ability Editor                               │
│ ───────────────│  │ ────────────────────────────────────────────│
│ [Search: ____] │  │                                              │
│                 │  │  Name: [Venomous Strike___]                  │
│ > Bite          │  │  Target: [SingleEnemy ▼]                     │
│   Slash         │  │                                              │
│   Fireball      │  │  Effects:                                    │
│   Heal          │  │  ┌────────────────────────────────────────┐ │
│   Poison Sting  │  │  │ 1. [Damage ▼] Value: [2] Duration: [-] │ │
│   Frost Breath  │  │  │ 2. [Poison ▼] Value: [1] Duration: [3] │ │
│                 │  │  │ [+ Add Effect]                          │ │
│ [+ New Ability] │  │  └────────────────────────────────────────┘ │
│                 │  │                                              │
│                 │  │  Animation: [PoisonSlash ▼]                  │
│                 │  │                                              │
│                 │  │  Description:                                │
│                 │  │  [Deal 2 damage. Poison for 1 over 3 turns.]│
│                 │  │                                              │
│                 │  │  [Save] [Delete] [Duplicate]                 │
└─────────────────┘  └──────────────────────────────────────────────┘
```

### Decks Tab
- Create/edit DeckData
- Drag cards into deck
- Show deck stats (mana curve, tribe distribution)
- Validate deck (min/max cards)

### Balance View Tab
```
┌─────────────────────────────────────────────────────────────────────────┐
│ Balance View - All Cards                          [Export] [Sort: Cost]│
├─────────────────────────────────────────────────────────────────────────┤
│ Name          │ Cost │ HP │ Off. Ability      │ Def. Ability    │ Tribe │
│───────────────│──────│────│───────────────────│─────────────────│───────│
│ Wolf          │  2   │  4 │ Bite (3 dmg)      │ Howl (+1 buff)  │ Beast │
│ Bear          │  4   │  7 │ Maul (5 dmg)      │ Roar (stun 1)   │ Beast │
│ Eagle         │  3   │  3 │ Dive (4 dmg)      │ Scout (draw 1)  │ Beast │
│ Snake         │  2   │  2 │ Venom (1+poison)  │ Shed (heal 2)   │ Beast │
│ Fire Imp      │  1   │  2 │ Spark (2 dmg)     │ Ember (1 to all)│ Demon │
│ Ice Golem     │  5   │  8 │ Frost (3+freeze)  │ Shield (absorb) │ Elem  │
│───────────────│──────│────│───────────────────│─────────────────│───────│
│                              Click any cell to edit inline              │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Architecture

### Scene Objects
```
DataManager (Scene)
├── Canvas
│   ├── TabBar
│   ├── CardsPanel
│   │   ├── CardList (ScrollView)
│   │   └── CardEditor
│   ├── AbilitiesPanel
│   │   ├── AbilityList (ScrollView)
│   │   └── AbilityEditor
│   ├── DecksPanel
│   └── BalancePanel
├── DataManagerController (Script)
├── CardPreviewRenderer (for card preview)
└── EventSystem
```

### Scripts
```
Assets/Scripts/DataManager/
├── DataManagerController.cs   - Main controller
├── CardListUI.cs              - Card list management
├── CardEditorUI.cs            - Card editing form
├── AbilityListUI.cs           - Ability list management
├── AbilityEditorUI.cs         - Ability editing form
├── EffectEditorUI.cs          - Effect row in ability editor
├── DeckEditorUI.cs            - Deck building interface
├── BalanceViewUI.cs           - Spreadsheet view
└── CardPreview.cs             - Live card preview
```

---

## Features

### Auto-Save
Changes save automatically when switching cards/abilities (with undo support).

### Validation
- Warn if card has no abilities assigned
- Warn if ability has no effects
- Warn if deck has wrong card count

### Search & Filter
- Search by name
- Filter by tribe, rarity, mana cost range
- Sort by any column

### Import/Export
- **Export CSV** - For spreadsheet editing
- **Import CSV** - Bulk update stats
- **Export JSON** - For backup/sharing

### Card Preview
Live preview showing how the card will look in-game, updates as you edit.

---

## Workflow

### Creating a New Card
1. Open DataManager scene
2. Click [+ New Card]
3. Fill in name, stats
4. Select or create offensive ability
5. Select or create defensive ability
6. Preview looks good → Save

### Balance Pass
1. Open Balance View tab
2. Sort by mana cost
3. Compare cards at each cost tier
4. Click cells to edit inline
5. Export CSV for team review

### Creating New Ability
1. Switch to Abilities tab
2. Click [+ New Ability]
3. Set target type
4. Add effects (Damage, then Poison, etc.)
5. Set animation
6. Write description
7. Save

---

## Benefits Over Folder Browsing

| Task | Folder Method | DataManager |
|------|---------------|-------------|
| Find a card | Navigate folders, open assets | Search, instant |
| Compare stats | Open multiple windows | Balance View table |
| Create card | Right-click, create, find abilities | One form |
| Edit ability | Find prefab, open, edit | Select from list |
| Build deck | Manual asset references | Drag and drop |

---

## Implementation Priority

### Phase 1 - Core Editor
- [ ] DataManager scene setup
- [ ] Card list and basic editor
- [ ] Ability list and basic editor
- [ ] ScriptableObject save/load

### Phase 2 - Enhanced Features
- [ ] Card preview renderer
- [ ] Effect stacking UI
- [ ] Deck builder
- [ ] Search and filter

### Phase 3 - Balance Tools
- [ ] Balance View spreadsheet
- [ ] CSV export/import
- [ ] Mana curve visualization
- [ ] Stat comparison charts

---

*Parent: [Game Design](./README.md)*
