using UnityEngine;
using UnityEngine.UI;

namespace KOA.UI
{
    /// <summary>
    /// Main controller for the Card Management scene.
    /// Handles navigation between Cards, Abilities, and Decks panels.
    /// </summary>
    public class CardManagementController : MonoBehaviour
    {
        [Header("Main Menu Buttons")]
        [SerializeField] private Button editCardsButton;
        [SerializeField] private Button editAbilitiesButton;
        [SerializeField] private Button editDecksButton;
        [SerializeField] private Button backToMainMenuButton;

        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject cardsPanel;
        [SerializeField] private GameObject abilitiesPanel;
        [SerializeField] private GameObject decksPanel;

        [Header("Panel Back Buttons")]
        [SerializeField] private Button cardsBackButton;
        [SerializeField] private Button abilitiesBackButton;
        [SerializeField] private Button decksBackButton;

        private void Start()
        {
            SetupButtonListeners();
            ShowMainMenu();
        }

        private void SetupButtonListeners()
        {
            // Main menu buttons
            if (editCardsButton != null)
                editCardsButton.onClick.AddListener(ShowCardsPanel);
            
            if (editAbilitiesButton != null)
                editAbilitiesButton.onClick.AddListener(ShowAbilitiesPanel);
            
            if (editDecksButton != null)
                editDecksButton.onClick.AddListener(ShowDecksPanel);
            
            if (backToMainMenuButton != null)
                backToMainMenuButton.onClick.AddListener(ExitToMainMenu);

            // Panel back buttons
            if (cardsBackButton != null)
                cardsBackButton.onClick.AddListener(ShowMainMenu);
            
            if (abilitiesBackButton != null)
                abilitiesBackButton.onClick.AddListener(ShowMainMenu);
            
            if (decksBackButton != null)
                decksBackButton.onClick.AddListener(ShowMainMenu);
        }

        #region Panel Navigation

        public void ShowMainMenu()
        {
            SetPanelActive(mainMenuPanel, true);
            SetPanelActive(cardsPanel, false);
            SetPanelActive(abilitiesPanel, false);
            SetPanelActive(decksPanel, false);
            
            Debug.Log("[CardManagement] Showing Main Menu");
        }

        public void ShowCardsPanel()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(cardsPanel, true);
            SetPanelActive(abilitiesPanel, false);
            SetPanelActive(decksPanel, false);
            
            Debug.Log("[CardManagement] Showing Cards Panel");
        }

        public void ShowAbilitiesPanel()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(cardsPanel, false);
            SetPanelActive(abilitiesPanel, true);
            SetPanelActive(decksPanel, false);
            
            Debug.Log("[CardManagement] Showing Abilities Panel");
        }

        public void ShowDecksPanel()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(cardsPanel, false);
            SetPanelActive(abilitiesPanel, false);
            SetPanelActive(decksPanel, true);
            
            Debug.Log("[CardManagement] Showing Decks Panel");
        }

        #endregion

        #region Utility Methods

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        private void ExitToMainMenu()
        {
            #if UNITY_EDITOR
            // In editor, just stop play mode
            Debug.Log("[CardManagement] Exiting Card Management (Editor)");
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            // In builds, load main menu scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            #endif
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Provide helpful warnings in the inspector
            if (mainMenuPanel == null)
                Debug.LogWarning("[CardManagement] Main Menu Panel not assigned!");
            if (cardsPanel == null)
                Debug.LogWarning("[CardManagement] Cards Panel not assigned!");
            if (abilitiesPanel == null)
                Debug.LogWarning("[CardManagement] Abilities Panel not assigned!");
            if (decksPanel == null)
                Debug.LogWarning("[CardManagement] Decks Panel not assigned!");
        }

        #endregion
    }
}
