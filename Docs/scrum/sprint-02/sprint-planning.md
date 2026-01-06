# Sprint 02 Planning

## Sprint Duration
Start: TBD
End: TBD

## Epic: Card Management Screen

A data-driven card system with a dedicated Unity scene for managing cards, abilities, and decks.

## Stories Overview

| Story | Title | Dependencies | Estimate |
|-------|-------|--------------|----------|
| 012 | Create Core Data ScriptableObjects | None | S |
| 013 | Create Base Effect Prefabs | 012 | S |
| 014 | Migrate CardController to CardData | 012 | M |
| 015 | Create CardLibrary System | 012, 014 | S |
| 016 | Create Card Management Scene Shell | None | S |
| 017 | Card List & Editor UI | 012, 016 | L |
| 018 | Ability List & Editor UI | 012, 016 | M |
| 019 | Create Initial Card/Ability Data Assets | 017, 018 | M |
| 020 | Deck Editor UI | 015, 016 | M |

## Dependency Graph

```
Foundation Track:              UI Track:
012 (ScriptableObjects) ──────► 016 (Scene Shell)
     │                              │
     ▼                              ▼
013 (Effect Prefabs)           017 (Card Editor UI)
     │                              │
     ▼                              ▼
014 (Migrate CardController)   018 (Ability Editor UI)
     │                              │
     ▼                              ▼
015 (CardLibrary) ─────────────► 019 (Initial Data)
                                    │
                                    ▼
                               020 (Deck Editor)
```

## Recommended Order

1. **012** - Core ScriptableObjects (enables everything)
2. **016** - Scene Shell (can be parallel with 012)
3. **013** - Base Effect Prefabs
4. **014** - Migrate CardController
5. **017** - Card List & Editor UI
6. **018** - Ability List & Editor UI
7. **015** - CardLibrary System
8. **019** - Initial Card/Ability Data
9. **020** - Deck Editor UI

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Editor-only APIs complex | Medium | Start simple, iterate |
| Migration breaks gameplay | High | Keep old system until fully migrated |
| UI complexity | Medium | Use Unity's built-in UI components |

## Notes

- Card Management scene is editor-only (not in builds)
- All data edits use `EditorUtility.SetDirty()` + `AssetDatabase.SaveAssets()`
- Effect prefabs are the only coded part; data is designer-editable
