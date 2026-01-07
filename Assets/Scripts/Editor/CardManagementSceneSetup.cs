using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using KOA.UI;
using TMPro;

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
            // Prevent running in play mode
            if (Application.isPlaying)
            {
                Debug.LogError("[CardManagement] Cannot create scene while in Play mode. Please exit Play mode first.");
                return;
            }
            
            // Create a new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Create the UI Canvas
            GameObject canvasObj = CreateCanvas();
            
            // Create the controller
            GameObject controllerObj = new GameObject("CardManagementController");
            var controller = controllerObj.AddComponent<CardManagementController>();
            
            // Create Main Menu Panel
            GameObject mainMenuPanel = CreateMainMenuPanel(canvasObj.transform);
            
            // Create Cards Panel with full editor UI
            GameObject cardsPanel = CreateCardEditorPanel(canvasObj.transform);
            cardsPanel.SetActive(false);
            
            // Create Abilities Panel with full editor UI
            GameObject abilitiesPanel = CreateAbilityEditorPanel(canvasObj.transform);
            abilitiesPanel.SetActive(false);
            
            // Create Decks Panel with full editor UI
            GameObject decksPanel = CreateDeckEditorPanel(canvasObj.transform);
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
            
            // Create CardListItem prefab if it doesn't exist
            CreateCardListItemPrefab();
            
            // Create AbilityListItem prefab if it doesn't exist
            CreateAbilityListItemPrefab();
            
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

        #region Card Editor Panel

        private static GameObject CreateCardEditorPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "CardsPanel");
            
            // Add CardEditorUI component
            var cardEditor = panel.AddComponent<CardEditorUI>();
            
            // Title
            GameObject titleObj = CreateText(panel.transform, "Title", "Card Editor", 36);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.92f);
            titleRect.anchorMax = new Vector2(0.5f, 0.98f);
            titleRect.sizeDelta = new Vector2(400, 50);
            titleRect.anchoredPosition = Vector2.zero;
            
            // Back button
            GameObject backBtn = CreateButton(panel.transform, "BackButton", "← Back", 40);
            RectTransform backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.02f, 0.92f);
            backRect.anchorMax = new Vector2(0.12f, 0.98f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            
            // Left Panel - Card List
            GameObject leftPanel = CreateCardListPanel(panel.transform);
            
            // Right Panel - Card Editor Form
            GameObject rightPanel = CreateCardFormPanel(panel.transform);
            
            // Confirmation Dialog
            GameObject confirmDialog = CreateConfirmDialog(panel.transform);
            
            // Wire up CardEditorUI
            SerializedObject so = new SerializedObject(cardEditor);
            
            // List panel references
            so.FindProperty("cardListContent").objectReferenceValue = leftPanel.transform.Find("ScrollView/Viewport/Content");
            so.FindProperty("searchField").objectReferenceValue = leftPanel.transform.Find("SearchField")?.GetComponent<TMP_InputField>();
            so.FindProperty("newCardButton").objectReferenceValue = leftPanel.transform.Find("NewCardButton")?.GetComponent<Button>();
            
            // Load prefab reference
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/CardListItem.prefab");
            so.FindProperty("cardListItemPrefab").objectReferenceValue = prefab;
            
            // Editor panel references
            so.FindProperty("editorPanel").objectReferenceValue = rightPanel;
            so.FindProperty("idField").objectReferenceValue = rightPanel.transform.Find("IdField")?.GetComponent<TMP_InputField>();
            so.FindProperty("nameField").objectReferenceValue = rightPanel.transform.Find("NameField")?.GetComponent<TMP_InputField>();
            so.FindProperty("healthField").objectReferenceValue = rightPanel.transform.Find("HealthField")?.GetComponent<TMP_InputField>();
            so.FindProperty("manaCostField").objectReferenceValue = rightPanel.transform.Find("ManaCostField")?.GetComponent<TMP_InputField>();
            so.FindProperty("descriptionField").objectReferenceValue = rightPanel.transform.Find("DescriptionField")?.GetComponent<TMP_InputField>();
            so.FindProperty("offensiveDropdown").objectReferenceValue = rightPanel.transform.Find("OffensiveDropdown")?.GetComponent<TMP_Dropdown>();
            so.FindProperty("defensiveDropdown").objectReferenceValue = rightPanel.transform.Find("DefensiveDropdown")?.GetComponent<TMP_Dropdown>();
            so.FindProperty("artworkImage").objectReferenceValue = rightPanel.transform.Find("ArtworkImage")?.GetComponent<Image>();
            so.FindProperty("selectArtworkButton").objectReferenceValue = rightPanel.transform.Find("SelectArtworkButton")?.GetComponent<Button>();
            so.FindProperty("saveButton").objectReferenceValue = rightPanel.transform.Find("SaveButton")?.GetComponent<Button>();
            so.FindProperty("deleteButton").objectReferenceValue = rightPanel.transform.Find("DeleteButton")?.GetComponent<Button>();
            
            // Confirm dialog references (children are inside DialogBox)
            so.FindProperty("confirmDialog").objectReferenceValue = confirmDialog;
            so.FindProperty("confirmText").objectReferenceValue = confirmDialog.transform.Find("DialogBox/Text")?.GetComponent<TMP_Text>();
            so.FindProperty("confirmYesButton").objectReferenceValue = confirmDialog.transform.Find("DialogBox/YesButton")?.GetComponent<Button>();
            so.FindProperty("confirmNoButton").objectReferenceValue = confirmDialog.transform.Find("DialogBox/NoButton")?.GetComponent<Button>();
            
            so.ApplyModifiedProperties();
            
            return panel;
        }

        private static GameObject CreateCardListPanel(Transform parent)
        {
            GameObject listPanel = new GameObject("ListPanel");
            listPanel.transform.SetParent(parent, false);
            
            RectTransform rect = listPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.05f);
            rect.anchorMax = new Vector2(0.3f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = listPanel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Search field
            GameObject searchField = CreateTMPInputField(listPanel.transform, "SearchField", "Search...");
            RectTransform searchRect = searchField.GetComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(0.05f, 0.92f);
            searchRect.anchorMax = new Vector2(0.95f, 0.98f);
            searchRect.offsetMin = Vector2.zero;
            searchRect.offsetMax = Vector2.zero;
            
            // Scroll view for card list
            GameObject scrollView = CreateScrollView(listPanel.transform, "ScrollView");
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.1f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.9f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            // New Card button
            GameObject newBtn = CreateTMPButton(listPanel.transform, "NewCardButton", "+ New Card", 40);
            RectTransform newRect = newBtn.GetComponent<RectTransform>();
            newRect.anchorMin = new Vector2(0.1f, 0.02f);
            newRect.anchorMax = new Vector2(0.9f, 0.08f);
            newRect.offsetMin = Vector2.zero;
            newRect.offsetMax = Vector2.zero;
            
            return listPanel;
        }

        private static GameObject CreateCardFormPanel(Transform parent)
        {
            GameObject formPanel = new GameObject("FormPanel");
            formPanel.transform.SetParent(parent, false);
            
            RectTransform rect = formPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.32f, 0.05f);
            rect.anchorMax = new Vector2(0.98f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = formPanel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Form fields with labels
            float y = 0.88f;
            float fieldHeight = 0.08f;
            float spacing = 0.1f;
            
            CreateFormField(formPanel.transform, "IdField", "ID:", ref y, fieldHeight, spacing);
            CreateFormField(formPanel.transform, "NameField", "Name:", ref y, fieldHeight, spacing);
            CreateFormField(formPanel.transform, "HealthField", "Health:", ref y, fieldHeight, spacing);
            CreateFormField(formPanel.transform, "ManaCostField", "Mana Cost:", ref y, fieldHeight, spacing);
            CreateFormDropdown(formPanel.transform, "OffensiveDropdown", "Offensive Ability:", ref y, fieldHeight, spacing);
            CreateFormDropdown(formPanel.transform, "DefensiveDropdown", "Defensive Ability:", ref y, fieldHeight, spacing);
            CreateFormField(formPanel.transform, "DescriptionField", "Description:", ref y, fieldHeight * 1.5f, spacing);
            
            // Artwork section
            CreateFormImage(formPanel.transform, "ArtworkImage", "Artwork:", ref y, 0.15f, spacing);
            
            // Save and Delete buttons
            GameObject saveBtn = CreateTMPButton(formPanel.transform, "SaveButton", "Save", 45);
            RectTransform saveRect = saveBtn.GetComponent<RectTransform>();
            saveRect.anchorMin = new Vector2(0.55f, 0.02f);
            saveRect.anchorMax = new Vector2(0.72f, 0.08f);
            saveRect.offsetMin = Vector2.zero;
            saveRect.offsetMax = Vector2.zero;
            
            GameObject deleteBtn = CreateTMPButton(formPanel.transform, "DeleteButton", "Delete", 45);
            RectTransform deleteRect = deleteBtn.GetComponent<RectTransform>();
            deleteRect.anchorMin = new Vector2(0.75f, 0.02f);
            deleteRect.anchorMax = new Vector2(0.92f, 0.08f);
            deleteRect.offsetMin = Vector2.zero;
            deleteRect.offsetMax = Vector2.zero;
            // Make delete button red-ish
            deleteBtn.GetComponent<Image>().color = new Color(0.5f, 0.25f, 0.25f, 1f);
            
            return formPanel;
        }

        private static void CreateFormField(Transform parent, string name, string label, ref float y, float height, float spacing)
        {
            // Label
            GameObject labelObj = CreateTMPText(parent, name + "Label", label, 20, TextAlignmentOptions.MidlineRight);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.02f, y - height);
            labelRect.anchorMax = new Vector2(0.25f, y);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            // Input field
            GameObject field = CreateTMPInputField(parent, name, "");
            RectTransform fieldRect = field.GetComponent<RectTransform>();
            fieldRect.anchorMin = new Vector2(0.27f, y - height);
            fieldRect.anchorMax = new Vector2(0.95f, y);
            fieldRect.offsetMin = Vector2.zero;
            fieldRect.offsetMax = Vector2.zero;
            
            y -= spacing;
        }

        private static void CreateConditionalFormField(Transform parent, string containerName, string fieldName, string label, ref float y, float height, float spacing)
        {
            // Container for visibility toggling
            GameObject container = new GameObject(containerName);
            container.transform.SetParent(parent, false);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0f, y - height);
            containerRect.anchorMax = new Vector2(1f, y);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            // Label
            GameObject labelObj = CreateTMPText(container.transform, fieldName + "Label", label, 20, TextAlignmentOptions.MidlineRight);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.02f, 0f);
            labelRect.anchorMax = new Vector2(0.25f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            // Input field
            GameObject field = CreateTMPInputField(container.transform, fieldName, "");
            RectTransform fieldRect = field.GetComponent<RectTransform>();
            fieldRect.anchorMin = new Vector2(0.27f, 0f);
            fieldRect.anchorMax = new Vector2(0.95f, 1f);
            fieldRect.offsetMin = Vector2.zero;
            fieldRect.offsetMax = Vector2.zero;
            
            y -= spacing;
        }

        private static void CreateFormDropdown(Transform parent, string name, string label, ref float y, float height, float spacing)
        {
            // Label
            GameObject labelObj = CreateTMPText(parent, name + "Label", label, 20, TextAlignmentOptions.MidlineRight);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.02f, y - height);
            labelRect.anchorMax = new Vector2(0.25f, y);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            // Dropdown
            GameObject dropdown = CreateTMPDropdown(parent, name);
            RectTransform dropRect = dropdown.GetComponent<RectTransform>();
            dropRect.anchorMin = new Vector2(0.27f, y - height);
            dropRect.anchorMax = new Vector2(0.95f, y);
            dropRect.offsetMin = Vector2.zero;
            dropRect.offsetMax = Vector2.zero;
            
            y -= spacing;
        }

        private static void CreateFormImage(Transform parent, string name, string label, ref float y, float height, float spacing)
        {
            // Label
            GameObject labelObj = CreateTMPText(parent, name + "Label", label, 20, TextAlignmentOptions.MidlineRight);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.02f, y - height);
            labelRect.anchorMax = new Vector2(0.25f, y);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            // Image preview
            GameObject imageObj = new GameObject(name);
            imageObj.transform.SetParent(parent, false);
            RectTransform imgRect = imageObj.AddComponent<RectTransform>();
            imgRect.anchorMin = new Vector2(0.27f, y - height);
            imgRect.anchorMax = new Vector2(0.42f, y);
            imgRect.offsetMin = Vector2.zero;
            imgRect.offsetMax = Vector2.zero;
            Image img = imageObj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.35f, 1f);
            img.preserveAspect = true;
            
            // Select button
            GameObject selectBtn = CreateTMPButton(parent, "SelectArtworkButton", "Select...", 30);
            RectTransform btnRect = selectBtn.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.44f, y - height * 0.5f);
            btnRect.anchorMax = new Vector2(0.6f, y - height * 0.2f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            y -= spacing + height * 0.5f;
        }

        private static GameObject CreateConfirmDialog(Transform parent)
        {
            GameObject dialog = new GameObject("ConfirmDialog");
            dialog.transform.SetParent(parent, false);
            
            RectTransform rect = dialog.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Semi-transparent background
            Image bg = dialog.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);
            
            // Dialog box
            GameObject box = new GameObject("DialogBox");
            box.transform.SetParent(dialog.transform, false);
            RectTransform boxRect = box.AddComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.3f, 0.35f);
            boxRect.anchorMax = new Vector2(0.7f, 0.65f);
            boxRect.offsetMin = Vector2.zero;
            boxRect.offsetMax = Vector2.zero;
            Image boxBg = box.AddComponent<Image>();
            boxBg.color = new Color(0.25f, 0.25f, 0.3f, 1f);
            
            // Text
            GameObject textObj = CreateTMPText(box.transform, "Text", "Are you sure?", 24, TextAlignmentOptions.Center);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.5f);
            textRect.anchorMax = new Vector2(0.9f, 0.85f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Yes button
            GameObject yesBtn = CreateTMPButton(box.transform, "YesButton", "Yes, Delete", 40);
            RectTransform yesRect = yesBtn.GetComponent<RectTransform>();
            yesRect.anchorMin = new Vector2(0.1f, 0.15f);
            yesRect.anchorMax = new Vector2(0.45f, 0.4f);
            yesRect.offsetMin = Vector2.zero;
            yesRect.offsetMax = Vector2.zero;
            yesBtn.GetComponent<Image>().color = new Color(0.5f, 0.25f, 0.25f, 1f);
            
            // No button
            GameObject noBtn = CreateTMPButton(box.transform, "NoButton", "Cancel", 40);
            RectTransform noRect = noBtn.GetComponent<RectTransform>();
            noRect.anchorMin = new Vector2(0.55f, 0.15f);
            noRect.anchorMax = new Vector2(0.9f, 0.4f);
            noRect.offsetMin = Vector2.zero;
            noRect.offsetMax = Vector2.zero;
            
            dialog.SetActive(false);
            return dialog;
        }

        private static GameObject CreateScrollView(Transform parent, string name)
        {
            GameObject scrollView = new GameObject(name);
            scrollView.transform.SetParent(parent, false);
            
            RectTransform rect = scrollView.AddComponent<RectTransform>();
            Image bg = scrollView.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            
            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            RectTransform vpRect = viewport.AddComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>();
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            
            // Vertical layout for content
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Content size fitter
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.viewport = vpRect;
            scroll.content = contentRect;
            
            return scrollView;
        }

        private static GameObject CreateTMPInputField(Transform parent, string name, string placeholder)
        {
            GameObject fieldObj = new GameObject(name);
            fieldObj.transform.SetParent(parent, false);
            
            RectTransform rect = fieldObj.AddComponent<RectTransform>();
            
            Image bg = fieldObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            
            TMP_InputField input = fieldObj.AddComponent<TMP_InputField>();
            
            // Text area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(fieldObj.transform, false);
            RectTransform taRect = textArea.AddComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.offsetMin = new Vector2(10, 5);
            taRect.offsetMax = new Vector2(-10, -5);
            
            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            RectTransform phRect = placeholderObj.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;
            TextMeshProUGUI phText = placeholderObj.AddComponent<TextMeshProUGUI>();
            phText.text = placeholder;
            phText.fontSize = 18;
            phText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            phText.alignment = TextAlignmentOptions.MidlineLeft;
            
            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            RectTransform txtRect = textObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.fontSize = 18;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.MidlineLeft;
            
            input.textViewport = taRect;
            input.textComponent = tmpText;
            input.placeholder = phText;
            
            return fieldObj;
        }

        private static GameObject CreateTMPText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = alignment;
            
            return textObj;
        }

        private static GameObject CreateTMPButton(Transform parent, string name, string text, float height)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.4f, 1f);
            
            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.5f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.3f, 1f);
            btn.colors = colors;
            
            // Button text with TMP
            GameObject textObj = CreateTMPText(btnObj.transform, "Text", text, (int)(height * 0.5f), TextAlignmentOptions.Center);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return btnObj;
        }

        private static GameObject CreateTMPDropdown(Transform parent, string name)
        {
            GameObject dropObj = new GameObject(name);
            dropObj.transform.SetParent(parent, false);
            
            RectTransform rect = dropObj.AddComponent<RectTransform>();
            
            Image bg = dropObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            
            TMP_Dropdown dropdown = dropObj.AddComponent<TMP_Dropdown>();
            
            // Label
            GameObject labelObj = CreateTMPText(dropObj.transform, "Label", "", 18, TextAlignmentOptions.MidlineLeft);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.offsetMin = new Vector2(10, 0);
            labelRect.offsetMax = new Vector2(-30, 0);
            
            // Arrow
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(dropObj.transform, false);
            RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1, 0.5f);
            arrowRect.anchorMax = new Vector2(1, 0.5f);
            arrowRect.sizeDelta = new Vector2(20, 20);
            arrowRect.anchoredPosition = new Vector2(-15, 0);
            Image arrowImg = arrowObj.AddComponent<Image>();
            arrowImg.color = Color.white;
            
            // Template (dropdown list) - opens UPWARD
            GameObject template = new GameObject("Template");
            template.transform.SetParent(dropObj.transform, false);
            RectTransform tempRect = template.AddComponent<RectTransform>();
            tempRect.anchorMin = new Vector2(0, 1);
            tempRect.anchorMax = new Vector2(1, 1);
            tempRect.pivot = new Vector2(0.5f, 0);
            tempRect.sizeDelta = new Vector2(0, 250); // Taller dropdown list
            tempRect.anchoredPosition = Vector2.zero;
            Image tempBg = template.AddComponent<Image>();
            tempBg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Add Canvas to template so it renders on top of everything
            Canvas templateCanvas = template.AddComponent<Canvas>();
            templateCanvas.overrideSorting = true;
            templateCanvas.sortingOrder = 30000; // Very high sorting order
            template.AddComponent<GraphicRaycaster>();
            
            ScrollRect scroll = template.AddComponent<ScrollRect>();
            
            // Viewport with padding
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            RectTransform vpRect = viewport.AddComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = new Vector2(0, 5);
            vpRect.offsetMax = new Vector2(0, -5);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>();
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            
            // Item
            GameObject item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            RectTransform itemRect = item.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.sizeDelta = new Vector2(0, 30);
            Toggle toggle = item.AddComponent<Toggle>();
            
            // Item background
            GameObject itemBg = new GameObject("Item Background");
            itemBg.transform.SetParent(item.transform, false);
            RectTransform itemBgRect = itemBg.AddComponent<RectTransform>();
            itemBgRect.anchorMin = Vector2.zero;
            itemBgRect.anchorMax = Vector2.one;
            itemBgRect.offsetMin = Vector2.zero;
            itemBgRect.offsetMax = Vector2.zero;
            Image itemBgImg = itemBg.AddComponent<Image>();
            itemBgImg.color = new Color(0.3f, 0.3f, 0.35f, 1f);
            
            // Item checkmark (hidden)
            GameObject checkmark = new GameObject("Item Checkmark");
            checkmark.transform.SetParent(item.transform, false);
            RectTransform checkRect = checkmark.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0, 0.5f);
            checkRect.anchorMax = new Vector2(0, 0.5f);
            checkRect.sizeDelta = new Vector2(20, 20);
            checkRect.anchoredPosition = new Vector2(15, 0);
            Image checkImg = checkmark.AddComponent<Image>();
            checkImg.color = Color.white;
            
            // Item label
            GameObject itemLabel = CreateTMPText(item.transform, "Item Label", "", 16, TextAlignmentOptions.MidlineLeft);
            RectTransform itemLabelRect = itemLabel.GetComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(25, 0);
            itemLabelRect.offsetMax = new Vector2(-10, 0);
            
            toggle.targetGraphic = itemBgImg;
            toggle.graphic = checkImg;
            
            scroll.viewport = vpRect;
            scroll.content = contentRect;
            
            dropdown.captionText = labelObj.GetComponent<TextMeshProUGUI>();
            dropdown.itemText = itemLabel.GetComponent<TextMeshProUGUI>();
            dropdown.template = tempRect;
            
            template.SetActive(false);
            
            return dropObj;
        }

        private static void CreateCardListItemPrefab()
        {
            string prefabPath = "Assets/Resources/UI/CardListItem.prefab";
            
            // Check if prefab already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log("[CardManagement] CardListItem prefab already exists");
                return;
            }
            
            // Ensure directory exists
            if (!System.IO.Directory.Exists("Assets/Resources/UI"))
            {
                System.IO.Directory.CreateDirectory("Assets/Resources/UI");
            }
            
            // Create prefab
            GameObject itemObj = new GameObject("CardListItem");
            
            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 40);
            
            Image bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.3f, 0.4f, 1f);
            
            Button btn = itemObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.5f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.3f, 1f);
            btn.colors = colors;
            
            // Add CardListItem component
            itemObj.AddComponent<CardListItem>();
            
            // Layout element for scroll view
            var layout = itemObj.AddComponent<LayoutElement>();
            layout.minHeight = 40;
            layout.preferredHeight = 40;
            
            // Name text
            GameObject textObj = new GameObject("NameText");
            textObj.transform.SetParent(itemObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 18;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            
            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(itemObj, prefabPath);
            Object.DestroyImmediate(itemObj);
            
            Debug.Log($"[CardManagement] Created CardListItem prefab at: {prefabPath}");
        }

        #endregion

        #region Ability Editor Panel

        private static GameObject CreateAbilityEditorPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "AbilitiesPanel");
            
            // Add AbilityEditorUI component
            var abilityEditor = panel.AddComponent<AbilityEditorUI>();
            
            // Title
            GameObject titleObj = CreateText(panel.transform, "Title", "Ability Editor", 36);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.92f);
            titleRect.anchorMax = new Vector2(0.5f, 0.98f);
            titleRect.sizeDelta = new Vector2(400, 50);
            titleRect.anchoredPosition = Vector2.zero;
            
            // Back button
            GameObject backBtn = CreateButton(panel.transform, "BackButton", "← Back", 40);
            RectTransform backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.02f, 0.92f);
            backRect.anchorMax = new Vector2(0.12f, 0.98f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            
            // Left Panel - Ability List
            GameObject leftPanel = CreateAbilityListPanel(panel.transform);
            
            // Right Panel - Ability Editor Form
            GameObject rightPanel = CreateAbilityFormPanel(panel.transform);
            
            // Confirmation Dialog
            GameObject confirmDialog = CreateConfirmDialog(panel.transform);
            confirmDialog.name = "AbilityConfirmDialog";
            
            // Wire up AbilityEditorUI
            SerializedObject so = new SerializedObject(abilityEditor);
            
            // List panel references
            so.FindProperty("abilityListContent").objectReferenceValue = leftPanel.transform.Find("ScrollView/Viewport/Content");
            so.FindProperty("searchField").objectReferenceValue = leftPanel.transform.Find("SearchField")?.GetComponent<TMP_InputField>();
            so.FindProperty("newAbilityButton").objectReferenceValue = leftPanel.transform.Find("NewAbilityButton")?.GetComponent<Button>();
            
            // Load prefab reference
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/AbilityListItem.prefab");
            so.FindProperty("abilityListItemPrefab").objectReferenceValue = prefab;
            
            // Editor panel references
            so.FindProperty("editorPanel").objectReferenceValue = rightPanel;
            so.FindProperty("idField").objectReferenceValue = rightPanel.transform.Find("IdField")?.GetComponent<TMP_InputField>();
            so.FindProperty("nameField").objectReferenceValue = rightPanel.transform.Find("NameField")?.GetComponent<TMP_InputField>();
            so.FindProperty("descriptionField").objectReferenceValue = rightPanel.transform.Find("DescriptionField")?.GetComponent<TMP_InputField>();
            so.FindProperty("behaviorDropdown").objectReferenceValue = rightPanel.transform.Find("BehaviorDropdown")?.GetComponent<TMP_Dropdown>();
            
            // Conditional field containers and fields
            so.FindProperty("damageFieldContainer").objectReferenceValue = rightPanel.transform.Find("DamageFieldContainer")?.gameObject;
            so.FindProperty("damageField").objectReferenceValue = rightPanel.transform.Find("DamageFieldContainer/DamageField")?.GetComponent<TMP_InputField>();
            so.FindProperty("healAmountFieldContainer").objectReferenceValue = rightPanel.transform.Find("HealAmountFieldContainer")?.gameObject;
            so.FindProperty("healAmountField").objectReferenceValue = rightPanel.transform.Find("HealAmountFieldContainer/HealAmountField")?.GetComponent<TMP_InputField>();
            so.FindProperty("durationFieldContainer").objectReferenceValue = rightPanel.transform.Find("DurationFieldContainer")?.gameObject;
            so.FindProperty("durationField").objectReferenceValue = rightPanel.transform.Find("DurationFieldContainer/DurationField")?.GetComponent<TMP_InputField>();
            
            // Other dropdowns
            so.FindProperty("targetTypeDropdown").objectReferenceValue = rightPanel.transform.Find("TargetTypeDropdown")?.GetComponent<TMP_Dropdown>();
            so.FindProperty("animationTypeDropdown").objectReferenceValue = rightPanel.transform.Find("AnimationTypeDropdown")?.GetComponent<TMP_Dropdown>();
            so.FindProperty("effectPrefabDropdown").objectReferenceValue = rightPanel.transform.Find("EffectPrefabDropdown")?.GetComponent<TMP_Dropdown>();
            so.FindProperty("saveButton").objectReferenceValue = rightPanel.transform.Find("SaveButton")?.GetComponent<Button>();
            so.FindProperty("deleteButton").objectReferenceValue = rightPanel.transform.Find("DeleteButton")?.GetComponent<Button>();
            
            // Confirm dialog references
            so.FindProperty("confirmDialog").objectReferenceValue = confirmDialog;
            so.FindProperty("confirmText").objectReferenceValue = confirmDialog.transform.Find("DialogBox/Text")?.GetComponent<TMP_Text>();
            so.FindProperty("confirmYesButton").objectReferenceValue = confirmDialog.transform.Find("DialogBox/YesButton")?.GetComponent<Button>();
            so.FindProperty("confirmNoButton").objectReferenceValue = confirmDialog.transform.Find("DialogBox/NoButton")?.GetComponent<Button>();
            
            so.ApplyModifiedProperties();
            
            return panel;
        }

        private static GameObject CreateAbilityListPanel(Transform parent)
        {
            GameObject listPanel = new GameObject("ListPanel");
            listPanel.transform.SetParent(parent, false);
            
            RectTransform rect = listPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.05f);
            rect.anchorMax = new Vector2(0.3f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = listPanel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Search field
            GameObject searchField = CreateTMPInputField(listPanel.transform, "SearchField", "Search...");
            RectTransform searchRect = searchField.GetComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(0.05f, 0.92f);
            searchRect.anchorMax = new Vector2(0.95f, 0.98f);
            searchRect.offsetMin = Vector2.zero;
            searchRect.offsetMax = Vector2.zero;
            
            // Scroll view for ability list
            GameObject scrollView = CreateScrollView(listPanel.transform, "ScrollView");
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.05f, 0.1f);
            scrollRect.anchorMax = new Vector2(0.95f, 0.9f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            // New Ability button
            GameObject newBtn = CreateTMPButton(listPanel.transform, "NewAbilityButton", "+ New Ability", 40);
            RectTransform newRect = newBtn.GetComponent<RectTransform>();
            newRect.anchorMin = new Vector2(0.1f, 0.02f);
            newRect.anchorMax = new Vector2(0.9f, 0.08f);
            newRect.offsetMin = Vector2.zero;
            newRect.offsetMax = Vector2.zero;
            
            return listPanel;
        }

        private static GameObject CreateAbilityFormPanel(Transform parent)
        {
            GameObject formPanel = new GameObject("FormPanel");
            formPanel.transform.SetParent(parent, false);
            
            RectTransform rect = formPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.32f, 0.05f);
            rect.anchorMax = new Vector2(0.98f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = formPanel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Form fields with labels
            float y = 0.95f;
            float fieldHeight = 0.06f;
            float spacing = 0.07f;
            
            CreateFormField(formPanel.transform, "IdField", "ID:", ref y, fieldHeight, spacing);
            CreateFormField(formPanel.transform, "NameField", "Name:", ref y, fieldHeight, spacing);
            CreateFormField(formPanel.transform, "DescriptionField", "Description:", ref y, fieldHeight, spacing);
            
            // Behavior dropdown (determines which conditional fields to show)
            CreateFormDropdown(formPanel.transform, "BehaviorDropdown", "Behavior:", ref y, fieldHeight, spacing);
            
            // Section header for conditional effect values
            y -= 0.02f;
            
            // Conditional fields - wrapped in containers for visibility toggling
            CreateConditionalFormField(formPanel.transform, "DamageFieldContainer", "DamageField", "Damage:", ref y, fieldHeight, spacing);
            CreateConditionalFormField(formPanel.transform, "HealAmountFieldContainer", "HealAmountField", "Heal Amount:", ref y, fieldHeight, spacing);
            CreateConditionalFormField(formPanel.transform, "DurationFieldContainer", "DurationField", "Duration:", ref y, fieldHeight, spacing);
            
            // Section header for targeting
            y -= 0.02f;
            
            CreateFormDropdown(formPanel.transform, "TargetTypeDropdown", "Target Type:", ref y, fieldHeight, spacing);
            CreateFormDropdown(formPanel.transform, "AnimationTypeDropdown", "Animation:", ref y, fieldHeight, spacing);
            CreateFormDropdown(formPanel.transform, "EffectPrefabDropdown", "Effect Prefab:", ref y, fieldHeight, spacing);
            
            // Save and Delete buttons
            GameObject saveBtn = CreateTMPButton(formPanel.transform, "SaveButton", "Save", 45);
            RectTransform saveRect = saveBtn.GetComponent<RectTransform>();
            saveRect.anchorMin = new Vector2(0.55f, 0.02f);
            saveRect.anchorMax = new Vector2(0.72f, 0.08f);
            saveRect.offsetMin = Vector2.zero;
            saveRect.offsetMax = Vector2.zero;
            
            GameObject deleteBtn = CreateTMPButton(formPanel.transform, "DeleteButton", "Delete", 45);
            RectTransform deleteRect = deleteBtn.GetComponent<RectTransform>();
            deleteRect.anchorMin = new Vector2(0.75f, 0.02f);
            deleteRect.anchorMax = new Vector2(0.92f, 0.08f);
            deleteRect.offsetMin = Vector2.zero;
            deleteRect.offsetMax = Vector2.zero;
            // Make delete button red-ish
            deleteBtn.GetComponent<Image>().color = new Color(0.5f, 0.25f, 0.25f, 1f);
            
            return formPanel;
        }

        private static void CreateAbilityListItemPrefab()
        {
            string prefabPath = "Assets/Resources/UI/AbilityListItem.prefab";
            
            // Check if prefab already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log("[CardManagement] AbilityListItem prefab already exists");
                return;
            }
            
            // Ensure directory exists
            if (!System.IO.Directory.Exists("Assets/Resources/UI"))
            {
                System.IO.Directory.CreateDirectory("Assets/Resources/UI");
            }
            
            // Create prefab
            GameObject itemObj = new GameObject("AbilityListItem");
            
            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 40);
            
            Image bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.3f, 0.4f, 1f);
            
            Button btn = itemObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.5f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.3f, 1f);
            btn.colors = colors;
            
            // Add AbilityListItem component
            itemObj.AddComponent<AbilityListItem>();
            
            // Layout element for scroll view
            var layout = itemObj.AddComponent<LayoutElement>();
            layout.minHeight = 40;
            layout.preferredHeight = 40;
            
            // Name text
            GameObject textObj = new GameObject("NameText");
            textObj.transform.SetParent(itemObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 18;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            
            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(itemObj, prefabPath);
            Object.DestroyImmediate(itemObj);
            
            Debug.Log($"[CardManagement] Created AbilityListItem prefab at: {prefabPath}");
        }

        #endregion

        #region Deck Editor Panel

        private static GameObject CreateDeckEditorPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "DecksPanel");
            
            // Add DeckEditorUI component
            var deckEditor = panel.AddComponent<DeckEditorUI>();
            
            // Title
            GameObject titleObj = CreateText(panel.transform, "Title", "Deck Editor", 36);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.92f);
            titleRect.anchorMax = new Vector2(0.5f, 0.98f);
            titleRect.sizeDelta = new Vector2(400, 50);
            titleRect.anchoredPosition = Vector2.zero;
            
            // Back button
            GameObject backBtn = CreateButton(panel.transform, "BackButton", "← Back", 40);
            RectTransform backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.02f, 0.92f);
            backRect.anchorMax = new Vector2(0.12f, 0.98f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            
            // Left Panel - Deck List
            GameObject deckListPanel = CreateDeckListPanel(panel.transform);
            
            // Middle Panel - Available Cards
            GameObject availableCardsPanel = CreateAvailableCardsPanelNew(panel.transform);
            
            // Right Panel - Deck Contents
            GameObject rightPanel = CreateDeckContentsPanelNew(panel.transform);
            
            // Stats row at bottom
            GameObject statsRow = CreateDeckStatsRow(panel.transform);
            
            // Confirmation Dialog
            GameObject confirmDialog = CreateConfirmDialog(panel.transform);
            confirmDialog.name = "DeckConfirmDialog";
            
            // Create prefabs
            CreateDeckCardItemPrefab();
            CreateDeckListItemPrefab();
            
            // Wire up DeckEditorUI
            SerializedObject so = new SerializedObject(deckEditor);
            
            // Deck list
            so.FindProperty("deckListContent").objectReferenceValue = deckListPanel.transform.Find("ScrollView/Viewport/Content");
            var deckListPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/DeckListItem.prefab");
            so.FindProperty("deckListItemPrefab").objectReferenceValue = deckListPrefab;
            so.FindProperty("newDeckButton").objectReferenceValue = deckListPanel.transform.Find("ButtonRow/NewDeckButton")?.GetComponent<Button>();
            so.FindProperty("deleteDeckButton").objectReferenceValue = deckListPanel.transform.Find("ButtonRow/DeleteDeckButton")?.GetComponent<Button>();
            
            // Available cards
            so.FindProperty("availableCardsContent").objectReferenceValue = availableCardsPanel.transform.Find("ScrollView/Viewport/Content");
            var availablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/AvailableCardItem.prefab");
            so.FindProperty("availableCardItemPrefab").objectReferenceValue = availablePrefab;
            
            // Deck contents
            so.FindProperty("deckContentsContent").objectReferenceValue = rightPanel.transform.Find("ScrollView/Viewport/Content");
            so.FindProperty("deckTitleText").objectReferenceValue = rightPanel.transform.Find("Title")?.GetComponent<TMP_Text>();
            var deckPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/DeckCardItem.prefab");
            so.FindProperty("deckCardItemPrefab").objectReferenceValue = deckPrefab;
            
            // Stats
            so.FindProperty("statsText").objectReferenceValue = statsRow.transform.Find("StatsText")?.GetComponent<TMP_Text>();
            so.FindProperty("saveButton").objectReferenceValue = statsRow.transform.Find("SaveButton")?.GetComponent<Button>();
            
            // Confirm dialog
            so.FindProperty("confirmDialog").objectReferenceValue = confirmDialog;
            so.FindProperty("confirmText").objectReferenceValue = confirmDialog.transform.Find("DialogBox/Text")?.GetComponent<TMP_Text>();
            so.FindProperty("confirmYesButton").objectReferenceValue = confirmDialog.transform.Find("DialogBox/YesButton")?.GetComponent<Button>();
            so.FindProperty("confirmNoButton").objectReferenceValue = confirmDialog.transform.Find("DialogBox/NoButton")?.GetComponent<Button>();
            
            so.ApplyModifiedProperties();
            
            return panel;
        }

        private static GameObject CreateDeckListPanel(Transform parent)
        {
            GameObject deckListPanel = new GameObject("DeckListPanel");
            deckListPanel.transform.SetParent(parent, false);
            
            RectTransform rect = deckListPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.1f);
            rect.anchorMax = new Vector2(0.22f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = deckListPanel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Title
            GameObject titleObj = CreateTMPText(deckListPanel.transform, "Title", "Decks", 22, TextAlignmentOptions.Center);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.92f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Scroll view for deck list
            GameObject scrollView = CreateScrollView(deckListPanel.transform, "ScrollView");
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.02f, 0.12f);
            scrollRect.anchorMax = new Vector2(0.98f, 0.9f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            // Button row at bottom
            GameObject buttonRow = new GameObject("ButtonRow");
            buttonRow.transform.SetParent(deckListPanel.transform, false);
            RectTransform buttonRowRect = buttonRow.AddComponent<RectTransform>();
            buttonRowRect.anchorMin = new Vector2(0.02f, 0.02f);
            buttonRowRect.anchorMax = new Vector2(0.98f, 0.1f);
            buttonRowRect.offsetMin = Vector2.zero;
            buttonRowRect.offsetMax = Vector2.zero;
            
            // New button
            GameObject newBtn = CreateTMPButton(buttonRow.transform, "NewDeckButton", "+ New", 25);
            RectTransform newRect = newBtn.GetComponent<RectTransform>();
            newRect.anchorMin = new Vector2(0f, 0f);
            newRect.anchorMax = new Vector2(0.48f, 1f);
            newRect.offsetMin = Vector2.zero;
            newRect.offsetMax = Vector2.zero;
            
            // Delete button
            GameObject delBtn = CreateTMPButton(buttonRow.transform, "DeleteDeckButton", "Delete", 25);
            RectTransform delRect = delBtn.GetComponent<RectTransform>();
            delRect.anchorMin = new Vector2(0.52f, 0f);
            delRect.anchorMax = new Vector2(1f, 1f);
            delRect.offsetMin = Vector2.zero;
            delRect.offsetMax = Vector2.zero;
            delBtn.GetComponent<Image>().color = new Color(0.5f, 0.25f, 0.25f, 1f);
            
            return deckListPanel;
        }

        private static GameObject CreateAvailableCardsPanelNew(Transform parent)
        {
            GameObject middlePanel = new GameObject("AvailableCardsPanel");
            middlePanel.transform.SetParent(parent, false);
            
            RectTransform rect = middlePanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.24f, 0.1f);
            rect.anchorMax = new Vector2(0.54f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = middlePanel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Title
            GameObject titleObj = CreateTMPText(middlePanel.transform, "Title", "Available Cards", 22, TextAlignmentOptions.Center);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.92f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Scroll view for cards
            GameObject scrollView = CreateScrollView(middlePanel.transform, "ScrollView");
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.02f, 0.02f);
            scrollRect.anchorMax = new Vector2(0.98f, 0.9f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            return middlePanel;
        }

        private static GameObject CreateDeckContentsPanelNew(Transform parent)
        {
            GameObject rightPanel = new GameObject("DeckContentsPanel");
            rightPanel.transform.SetParent(parent, false);
            
            RectTransform rect = rightPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.56f, 0.1f);
            rect.anchorMax = new Vector2(0.98f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = rightPanel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Title
            GameObject titleObj = CreateTMPText(rightPanel.transform, "Title", "Deck Contents (0/30)", 22, TextAlignmentOptions.Center);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.92f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Scroll view for deck cards
            GameObject scrollView = CreateScrollView(rightPanel.transform, "ScrollView");
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.02f, 0.02f);
            scrollRect.anchorMax = new Vector2(0.98f, 0.9f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            return rightPanel;
        }

        private static void CreateDeckListItemPrefab()
        {
            string prefabPath = "Assets/Resources/UI/DeckListItem.prefab";
            
            // Check if prefab already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log($"[CardManagement] Prefab already exists: {prefabPath}");
                return;
            }
            
            // Ensure directory exists
            string dir = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            
            // Create prefab
            GameObject itemObj = new GameObject("DeckListItem");
            
            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 60);
            
            Image bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.3f, 0.4f, 1f);
            
            // Layout element for scroll view
            var layout = itemObj.AddComponent<LayoutElement>();
            layout.minHeight = 60;
            layout.preferredHeight = 60;
            
            // Name text
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(itemObj.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.5f);
            nameRect.anchorMax = new Vector2(0.95f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 18;
            nameTmp.color = Color.white;
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            nameTmp.text = "Deck Name";
            
            // Count text
            GameObject countObj = new GameObject("CountText");
            countObj.transform.SetParent(itemObj.transform, false);
            RectTransform countRect = countObj.AddComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.05f, 0.05f);
            countRect.anchorMax = new Vector2(0.95f, 0.5f);
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI countTmp = countObj.AddComponent<TextMeshProUGUI>();
            countTmp.fontSize = 14;
            countTmp.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            countTmp.alignment = TextAlignmentOptions.MidlineLeft;
            countTmp.text = "10 cards";
            
            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(itemObj, prefabPath);
            Object.DestroyImmediate(itemObj);
            
            Debug.Log($"[CardManagement] Created prefab: {prefabPath}");
        }

        private static GameObject CreateDeckSelectionRow(Transform parent)
        {
            GameObject row = new GameObject("DeckSelectionRow");
            row.transform.SetParent(parent, false);
            
            RectTransform rect = row.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.85f);
            rect.anchorMax = new Vector2(0.98f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Deck label
            GameObject labelObj = CreateTMPText(row.transform, "DeckLabel", "Deck:", 20, TextAlignmentOptions.MidlineRight);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.02f, 0.1f);
            labelRect.anchorMax = new Vector2(0.1f, 0.9f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            // Deck dropdown
            GameObject dropdown = CreateTMPDropdown(row.transform, "DeckDropdown");
            RectTransform dropRect = dropdown.GetComponent<RectTransform>();
            dropRect.anchorMin = new Vector2(0.11f, 0.1f);
            dropRect.anchorMax = new Vector2(0.5f, 0.9f);
            dropRect.offsetMin = Vector2.zero;
            dropRect.offsetMax = Vector2.zero;
            
            // New Deck button
            GameObject newBtn = CreateTMPButton(row.transform, "NewDeckButton", "+ New Deck", 30);
            RectTransform newRect = newBtn.GetComponent<RectTransform>();
            newRect.anchorMin = new Vector2(0.52f, 0.1f);
            newRect.anchorMax = new Vector2(0.7f, 0.9f);
            newRect.offsetMin = Vector2.zero;
            newRect.offsetMax = Vector2.zero;
            
            // Delete button
            GameObject delBtn = CreateTMPButton(row.transform, "DeleteDeckButton", "Delete", 30);
            RectTransform delRect = delBtn.GetComponent<RectTransform>();
            delRect.anchorMin = new Vector2(0.72f, 0.1f);
            delRect.anchorMax = new Vector2(0.85f, 0.9f);
            delRect.offsetMin = Vector2.zero;
            delRect.offsetMax = Vector2.zero;
            delBtn.GetComponent<Image>().color = new Color(0.5f, 0.25f, 0.25f, 1f);
            
            return row;
        }

        private static GameObject CreateAvailableCardsPanel(Transform parent)
        {
            GameObject leftPanel = new GameObject("AvailableCardsPanel");
            leftPanel.transform.SetParent(parent, false);
            
            RectTransform rect = leftPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.1f);
            rect.anchorMax = new Vector2(0.48f, 0.83f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = leftPanel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Title
            GameObject titleObj = CreateTMPText(leftPanel.transform, "Title", "Available Cards", 22, TextAlignmentOptions.Center);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.92f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Scroll view for cards
            GameObject scrollView = CreateScrollView(leftPanel.transform, "ScrollView");
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.02f, 0.02f);
            scrollRect.anchorMax = new Vector2(0.98f, 0.9f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            return leftPanel;
        }

        private static GameObject CreateDeckContentsPanel(Transform parent)
        {
            GameObject rightPanel = new GameObject("DeckContentsPanel");
            rightPanel.transform.SetParent(parent, false);
            
            RectTransform rect = rightPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.52f, 0.1f);
            rect.anchorMax = new Vector2(0.98f, 0.83f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = rightPanel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Title
            GameObject titleObj = CreateTMPText(rightPanel.transform, "Title", "Deck Contents (0/30)", 22, TextAlignmentOptions.Center);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.92f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            // Scroll view for deck cards
            GameObject scrollView = CreateScrollView(rightPanel.transform, "ScrollView");
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.02f, 0.02f);
            scrollRect.anchorMax = new Vector2(0.98f, 0.9f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            return rightPanel;
        }

        private static GameObject CreateDeckStatsRow(Transform parent)
        {
            GameObject row = new GameObject("StatsRow");
            row.transform.SetParent(parent, false);
            
            RectTransform rect = row.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.02f);
            rect.anchorMax = new Vector2(0.98f, 0.08f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
            // Stats text
            GameObject statsObj = CreateTMPText(row.transform, "StatsText", "0 cards │ Avg Mana: - │ Avg HP: -", 18, TextAlignmentOptions.MidlineLeft);
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.02f, 0.1f);
            statsRect.anchorMax = new Vector2(0.7f, 0.9f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;
            
            // Save button
            GameObject saveBtn = CreateTMPButton(row.transform, "SaveButton", "Save Deck", 35);
            RectTransform saveRect = saveBtn.GetComponent<RectTransform>();
            saveRect.anchorMin = new Vector2(0.75f, 0.1f);
            saveRect.anchorMax = new Vector2(0.95f, 0.9f);
            saveRect.offsetMin = Vector2.zero;
            saveRect.offsetMax = Vector2.zero;
            
            return row;
        }

        private static void CreateDeckCardItemPrefab()
        {
            // Create Available Card Item prefab
            CreateCardItemPrefab("Assets/Resources/UI/AvailableCardItem.prefab", "AddButton", "+");
            
            // Create Deck Card Item prefab
            CreateCardItemPrefab("Assets/Resources/UI/DeckCardItem.prefab", "RemoveButton", "-");
        }

        private static void CreateCardItemPrefab(string prefabPath, string buttonName, string buttonText)
        {
            // Check if prefab already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log($"[CardManagement] Prefab already exists: {prefabPath}");
                return;
            }
            
            // Ensure directory exists
            string dir = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            
            // Create prefab
            GameObject itemObj = new GameObject(System.IO.Path.GetFileNameWithoutExtension(prefabPath));
            
            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 50);
            
            Image bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.3f, 0.4f, 1f);
            
            // Layout element for scroll view
            var layout = itemObj.AddComponent<LayoutElement>();
            layout.minHeight = 50;
            layout.preferredHeight = 50;
            
            // Name text
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(itemObj.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.02f, 0.5f);
            nameRect.anchorMax = new Vector2(0.5f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 18;
            nameTmp.color = Color.white;
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            nameTmp.text = "Card Name";
            
            // Stats text
            GameObject statsObj = new GameObject("StatsText");
            statsObj.transform.SetParent(itemObj.transform, false);
            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.02f, 0.05f);
            statsRect.anchorMax = new Vector2(0.5f, 0.5f);
            statsRect.offsetMin = Vector2.zero;
            statsRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI statsTmp = statsObj.AddComponent<TextMeshProUGUI>();
            statsTmp.fontSize = 14;
            statsTmp.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            statsTmp.alignment = TextAlignmentOptions.MidlineLeft;
            statsTmp.text = "2⬡ 3♥";
            
            // Action button (+/-)
            GameObject btnObj = new GameObject(buttonName);
            btnObj.transform.SetParent(itemObj.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.85f, 0.15f);
            btnRect.anchorMax = new Vector2(0.98f, 0.85f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = buttonText == "+" ? new Color(0.2f, 0.5f, 0.2f, 1f) : new Color(0.5f, 0.2f, 0.2f, 1f);
            
            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = buttonText == "+" ? new Color(0.3f, 0.6f, 0.3f, 1f) : new Color(0.6f, 0.3f, 0.3f, 1f);
            btn.colors = colors;
            
            // Button text
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnTmp.fontSize = 24;
            btnTmp.color = Color.white;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.text = buttonText;
            
            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(itemObj, prefabPath);
            Object.DestroyImmediate(itemObj);
            
            Debug.Log($"[CardManagement] Created prefab: {prefabPath}");
        }

        #endregion
    }
}

