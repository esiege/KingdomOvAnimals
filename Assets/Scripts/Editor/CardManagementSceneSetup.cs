using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using KOA.UI;

namespace KOA.Editor
{
    /// <summary>
    /// Editor tool to create and set up the Card Management scene.
    /// Accessible via menu: Tools > KingdomOvAnimals > Create Card Management Scene
    /// </summary>
    public static class CardManagementSceneSetup
    {
        [MenuItem("Tools/KingdomOvAnimals/Create Card Management Scene")]
        public static void CreateCardManagementScene()
        {
            // Create a new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Create the UI Canvas
            GameObject canvasObj = CreateCanvas();
            
            // Create the controller
            GameObject controllerObj = new GameObject("CardManagementController");
            var controller = controllerObj.AddComponent<CardManagementController>();
            
            // Create Main Menu Panel
            GameObject mainMenuPanel = CreateMainMenuPanel(canvasObj.transform);
            
            // Create Cards Panel (hidden by default)
            GameObject cardsPanel = CreateEditorPanel(canvasObj.transform, "CardsPanel", "Card Editor");
            cardsPanel.SetActive(false);
            
            // Create Abilities Panel (hidden by default)
            GameObject abilitiesPanel = CreateEditorPanel(canvasObj.transform, "AbilitiesPanel", "Ability Editor");
            abilitiesPanel.SetActive(false);
            
            // Create Decks Panel (hidden by default)
            GameObject decksPanel = CreateEditorPanel(canvasObj.transform, "DecksPanel", "Deck Editor");
            decksPanel.SetActive(false);
            
            // Wire up the controller using SerializedObject
            SerializedObject so = new SerializedObject(controller);
            
            // Main Menu buttons (inside ButtonContainer)
            Transform buttonContainer = mainMenuPanel.transform.Find("ButtonContainer");
            so.FindProperty("editCardsButton").objectReferenceValue = buttonContainer?.Find("EditCardsButton")?.GetComponent<Button>();
            so.FindProperty("editAbilitiesButton").objectReferenceValue = buttonContainer?.Find("EditAbilitiesButton")?.GetComponent<Button>();
            so.FindProperty("editDecksButton").objectReferenceValue = buttonContainer?.Find("EditDecksButton")?.GetComponent<Button>();
            so.FindProperty("backToMainMenuButton").objectReferenceValue = mainMenuPanel.transform.Find("ExitButton")?.GetComponent<Button>();
            
            // Panels
            so.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
            so.FindProperty("cardsPanel").objectReferenceValue = cardsPanel;
            so.FindProperty("abilitiesPanel").objectReferenceValue = abilitiesPanel;
            so.FindProperty("decksPanel").objectReferenceValue = decksPanel;
            
            // Back buttons
            so.FindProperty("cardsBackButton").objectReferenceValue = cardsPanel.transform.Find("BackButton")?.GetComponent<Button>();
            so.FindProperty("abilitiesBackButton").objectReferenceValue = abilitiesPanel.transform.Find("BackButton")?.GetComponent<Button>();
            so.FindProperty("decksBackButton").objectReferenceValue = decksPanel.transform.Find("BackButton")?.GetComponent<Button>();
            
            so.ApplyModifiedProperties();
            
            // Create EventSystem if it doesn't exist
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            // Save the scene
            string scenePath = "Assets/Scenes/CardManagement.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log($"[CardManagement] Scene created and saved to: {scenePath}");
            Debug.Log("[CardManagement] Don't forget to add the scene to Build Settings!");
            
            Selection.activeGameObject = controllerObj;
        }

        private static GameObject CreateCanvas()
        {
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Configure CanvasScaler for consistent UI
            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            return canvasObj;
        }

        private static GameObject CreateMainMenuPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "MainMenuPanel");
            
            // Title
            GameObject titleObj = CreateText(panel.transform, "Title", "Card Management", 48);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.9f);
            titleRect.sizeDelta = new Vector2(600, 80);
            titleRect.anchoredPosition = Vector2.zero;
            
            // Button container
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(panel.transform, false);
            RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.3f, 0.3f);
            containerRect.anchorMax = new Vector2(0.7f, 0.7f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            // Vertical layout
            var layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Buttons
            CreateButton(buttonContainer.transform, "EditCardsButton", "Edit Cards", 60);
            CreateButton(buttonContainer.transform, "EditAbilitiesButton", "Edit Abilities", 60);
            CreateButton(buttonContainer.transform, "EditDecksButton", "Edit Starting Decks", 60);
            
            // Exit button at bottom
            GameObject exitBtn = CreateButton(panel.transform, "ExitButton", "Exit", 50);
            RectTransform exitRect = exitBtn.GetComponent<RectTransform>();
            exitRect.anchorMin = new Vector2(0.4f, 0.1f);
            exitRect.anchorMax = new Vector2(0.6f, 0.15f);
            exitRect.offsetMin = Vector2.zero;
            exitRect.offsetMax = Vector2.zero;
            
            return panel;
        }

        private static GameObject CreateEditorPanel(Transform parent, string name, string title)
        {
            GameObject panel = CreatePanel(parent, name);
            
            // Title
            GameObject titleObj = CreateText(panel.transform, "Title", title, 36);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.9f);
            titleRect.anchorMax = new Vector2(0.5f, 0.95f);
            titleRect.sizeDelta = new Vector2(400, 50);
            titleRect.anchoredPosition = Vector2.zero;
            
            // Placeholder text
            GameObject placeholderObj = CreateText(panel.transform, "Placeholder", "(Editor UI will be added in Story 017-020)", 24);
            RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = new Vector2(0.5f, 0.5f);
            placeholderRect.anchorMax = new Vector2(0.5f, 0.5f);
            placeholderRect.sizeDelta = new Vector2(600, 50);
            placeholderRect.anchoredPosition = Vector2.zero;
            
            // Back button
            GameObject backBtn = CreateButton(panel.transform, "BackButton", "← Back", 40);
            RectTransform backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.05f, 0.9f);
            backRect.anchorMax = new Vector2(0.15f, 0.95f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            
            return panel;
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            
            return panel;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, float height)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, height);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.4f, 1f);
            
            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.5f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.3f, 1f);
            btn.colors = colors;
            
            // Button text
            GameObject textObj = CreateText(btnObj.transform, "Text", text, (int)(height * 0.5f));
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Add layout element for vertical layout
            var layoutElement = btnObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = height;
            
            return btnObj;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            
            Text textComp = textObj.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;
            textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            return textObj;
        }
    }
}
