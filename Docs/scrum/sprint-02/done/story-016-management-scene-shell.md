# Story 016: Create Card Management Scene Shell

## Status: Complete ✓
## Sprint: 02
## Dependencies: None
## Started: 2026-01-06
## Completed: 2026-01-06

---

## User Story

**As a** designer  
**I want** a Card Management scene with a main menu  
**So that** I can navigate to different editors

---

## Description

- New scene "CardManagement" in Scenes folder
- Main menu with buttons: Edit Cards, Edit Abilities, Edit Decks
- Panel navigation system (show/hide panels)
- Back button to return to main menu
- Scene NOT added to build settings

---

## Acceptance Criteria

- [ ] I see "Card Management" title and three buttons on scene start
- [ ] Clicking "Edit Cards" shows the card editor panel
- [ ] Clicking "Edit Abilities" shows the ability editor panel
- [ ] Clicking "Edit Decks" shows the deck editor panel
- [ ] Back button returns to main menu from any panel

---

## Technical Notes

### Scene Hierarchy
```
CardManagement (Scene)
├── Canvas
│   ├── MainMenuPanel
│   │   ├── Title: "Card Management"
│   │   ├── EditCardsButton
│   │   ├── EditAbilitiesButton
│   │   └── EditDecksButton
│   ├── CardEditorPanel (hidden)
│   ├── AbilityEditorPanel (hidden)
│   └── DeckEditorPanel (hidden)
├── EventSystem
└── CardManagementController
```

### CardManagementController.cs
```csharp
public class CardManagementController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject cardEditorPanel;
    [SerializeField] private GameObject abilityEditorPanel;
    [SerializeField] private GameObject deckEditorPanel;
    
    public void ShowCardEditor()
    {
        HideAllPanels();
        cardEditorPanel.SetActive(true);
    }
    
    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }
    
    private void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        cardEditorPanel.SetActive(false);
        abilityEditorPanel.SetActive(false);
        deckEditorPanel.SetActive(false);
    }
}
```

---

## Tasks

- [ ] Create CardManagement scene
- [ ] Set up Canvas with panels
- [ ] Create CardManagementController
- [ ] Wire up navigation buttons
- [ ] Verify NOT in build settings
