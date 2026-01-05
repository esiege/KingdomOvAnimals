#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor utility to add/update disconnect UI to the DuelScreen scene.
/// Access via menu: Tools > KingdomOvAnimals > Setup Disconnect UI
/// </summary>
public class DuelScreenSetup : EditorWindow
{
    [MenuItem("Tools/KingdomOvAnimals/Setup Disconnect UI")]
    public static void ShowWindow()
    {
        GetWindow<DuelScreenSetup>("Disconnect UI Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Disconnect UI Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This will add disconnect handling UI to the current scene:\n" +
            "• Disconnect Panel (overlay)\n" +
            "• Status text with timer\n" +
            "• Wire up to EncounterController\n\n" +
            "Make sure you have the DuelScreen scene open!",
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("Add Disconnect UI", GUILayout.Height(40)))
        {
            AddDisconnectUI();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Wire Up Existing UI", GUILayout.Height(30)))
        {
            WireUpExistingUI();
        }
    }

    private static void AddDisconnectUI()
    {
        // Find the main UI Canvas (child of "UI" parent, not card canvases)
        Canvas canvas = FindMainUICanvas();
        if (canvas == null)
        {
            Debug.LogError("[DuelScreenSetup] No Canvas found under 'UI' object! Please ensure UI hierarchy exists.");
            return;
        }

        // Check if disconnect panel already exists
        Transform existingPanel = canvas.transform.Find("DisconnectPanel");
        if (existingPanel != null)
        {
            Debug.LogWarning("[DuelScreenSetup] DisconnectPanel already exists. Use 'Wire Up Existing UI' to reconnect.");
            return;
        }

        // Create Disconnect Panel
        GameObject disconnectPanel = new GameObject("DisconnectPanel");
        disconnectPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = disconnectPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        // Semi-transparent background
        Image panelImage = disconnectPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        // Create status text
        GameObject textObj = new GameObject("DisconnectStatusText");
        textObj.transform.SetParent(disconnectPanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(600, 300);
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI statusText = textObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Opponent disconnected!\nWaiting for reconnect... 30s";
        statusText.fontSize = 36;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = Color.white;
        
        // Hide panel by default
        disconnectPanel.SetActive(false);
        
        // Wire up to EncounterController
        WireUpToEncounterController(disconnectPanel, statusText);
        
        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("[DuelScreenSetup] Disconnect UI added successfully!");
    }

    private static void WireUpExistingUI()
    {
        Canvas canvas = FindMainUICanvas();
        if (canvas == null)
        {
            Debug.LogError("[DuelScreenSetup] No Canvas found under 'UI' object!");
            return;
        }

        Transform panelTransform = canvas.transform.Find("DisconnectPanel");
        if (panelTransform == null)
        {
            Debug.LogError("[DuelScreenSetup] DisconnectPanel not found! Use 'Add Disconnect UI' first.");
            return;
        }

        GameObject disconnectPanel = panelTransform.gameObject;
        TextMeshProUGUI statusText = disconnectPanel.GetComponentInChildren<TextMeshProUGUI>();
        
        if (statusText == null)
        {
            Debug.LogError("[DuelScreenSetup] No TextMeshProUGUI found in DisconnectPanel!");
            return;
        }

        WireUpToEncounterController(disconnectPanel, statusText);
        
        Debug.Log("[DuelScreenSetup] Disconnect UI wired up successfully!");
    }

    private static void WireUpToEncounterController(GameObject disconnectPanel, TextMeshProUGUI statusText)
    {
        EncounterController encounterController = FindObjectOfType<EncounterController>();
        if (encounterController == null)
        {
            Debug.LogWarning("[DuelScreenSetup] No EncounterController found in scene! You'll need to wire up manually.");
            return;
        }

        SerializedObject so = new SerializedObject(encounterController);
        so.FindProperty("disconnectPanel").objectReferenceValue = disconnectPanel;
        so.FindProperty("disconnectStatusText").objectReferenceValue = statusText;
        so.ApplyModifiedProperties();
        
        Debug.Log("[DuelScreenSetup] Wired DisconnectPanel to EncounterController");
    }

    /// <summary>
    /// Finds the main UI Canvas (under the "UI" parent object, not card canvases)
    /// </summary>
    private static Canvas FindMainUICanvas()
    {
        // First try to find the "UI" GameObject
        GameObject uiParent = GameObject.Find("UI");
        if (uiParent != null)
        {
            Canvas canvas = uiParent.GetComponentInChildren<Canvas>();
            if (canvas != null)
                return canvas;
        }

        // Fallback: look for a root-level Canvas (not nested under Cards)
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            // Skip canvases that are children of "Cards" or have "Card" in parent hierarchy
            Transform parent = canvas.transform.parent;
            bool isCardCanvas = false;
            while (parent != null)
            {
                if (parent.name == "Cards" || parent.name.Contains("Card"))
                {
                    isCardCanvas = true;
                    break;
                }
                parent = parent.parent;
            }
            
            if (!isCardCanvas)
                return canvas;
        }

        return null;
    }
}
#endif
