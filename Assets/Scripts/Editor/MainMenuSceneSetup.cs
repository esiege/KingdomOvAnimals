#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Editor utility to create the MainMenu scene with all required UI and networking components.
/// Access via menu: Tools > KingdomOvAnimals > Create Main Menu Scene
/// </summary>
public class MainMenuSceneSetup : EditorWindow
{
    [MenuItem("Tools/KingdomOvAnimals/Create Main Menu Scene")]
    public static void ShowWindow()
    {
        GetWindow<MainMenuSceneSetup>("Main Menu Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Main Menu Scene Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This will create a complete MainMenu scene with:\n" +
            "• NetworkManager prefab\n" +
            "• PlayerConnectionHandler\n" +
            "• Main Menu UI (Play/Quit)\n" +
            "• Lobby UI (Status, Player Count, Cancel)\n" +
            "• Auto-matchmaking (first player hosts, second joins)",
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("Create Main Menu Scene", GUILayout.Height(40)))
        {
            CreateMainMenuScene();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Add Scenes to Build Settings", GUILayout.Height(30)))
        {
            AddScenesToBuildSettings();
        }
    }

    private static void CreateMainMenuScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Remove default camera and light (we'll create our own setup)
        var mainCam = GameObject.Find("Main Camera");
        if (mainCam != null) DestroyImmediate(mainCam);
        
        // Create camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera camera = cameraObj.AddComponent<Camera>();
        cameraObj.AddComponent<AudioListener>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        camera.orthographic = true;
        cameraObj.tag = "MainCamera";

        // Add NetworkManager prefab
        string networkManagerPath = "Assets/FishNet/Demos/Prefabs/NetworkManager.prefab";
        GameObject networkManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(networkManagerPath);
        GameObject networkManagerInstance = null;
        if (networkManagerPrefab != null)
        {
            networkManagerInstance = (GameObject)PrefabUtility.InstantiatePrefab(networkManagerPrefab);
            
            // Assign DefaultPrefabObjects to NetworkManager
            var networkManager = networkManagerInstance.GetComponent<FishNet.Managing.NetworkManager>();
            if (networkManager != null)
            {
                string[] guids = AssetDatabase.FindAssets("t:DefaultPrefabObjects");
                if (guids.Length > 0)
                {
                    string dpPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var defaultPrefabs = AssetDatabase.LoadAssetAtPath<FishNet.Managing.Object.DefaultPrefabObjects>(dpPath);
                    
                    SerializedObject nmSO = new SerializedObject(networkManager);
                    nmSO.FindProperty("_spawnablePrefabs").objectReferenceValue = defaultPrefabs;
                    nmSO.ApplyModifiedProperties();
                    
                    Debug.Log("[MainMenuSetup] Assigned DefaultPrefabObjects to NetworkManager");
                }
            }
            
            Debug.Log("[MainMenuSetup] Added NetworkManager prefab");
        }
        else
        {
            Debug.LogWarning("[MainMenuSetup] NetworkManager prefab not found at: " + networkManagerPath);
        }

        // Add PlayerConnectionHandler
        GameObject connectionHandler = new GameObject("PlayerConnectionHandler");
        PlayerConnectionHandler handler = connectionHandler.AddComponent<PlayerConnectionHandler>();
        
        // Assign NetworkPlayer prefab if it exists
        string playerPrefabPath = "Assets/Prefabs/Network/NetworkPlayer.prefab";
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
        if (playerPrefab != null)
        {
            var nob = playerPrefab.GetComponent<FishNet.Object.NetworkObject>();
            SerializedObject so = new SerializedObject(handler);
            so.FindProperty("playerPrefab").objectReferenceValue = nob;
            so.ApplyModifiedProperties();
        }

        // Add ConsoleLogToFile for debugging
        GameObject loggerObj = new GameObject("ConsoleLogger");
        ConsoleLogToFile logger = loggerObj.AddComponent<ConsoleLogToFile>();
        logger.logFilePath = "Docs/log.txt";
        logger.includeStackTrace = true;
        logger.includeTimestamp = true;
        Debug.Log("[MainMenuSetup] Added ConsoleLogToFile component");

        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Create Main Menu Panel
        GameObject mainMenuPanel = CreatePanel(canvasObj.transform, "MainMenuPanel");
        
        // Title
        CreateText(mainMenuPanel.transform, "TitleText", "Kingdom Ov Animals", 48, new Vector2(0, 150));
        
        // Play Button (handles auto-matchmaking)
        Button joinBtn = CreateButton(mainMenuPanel.transform, "PlayButton", "Play", new Vector2(0, 20));
        
        // Quit Button
        Button quitBtn = CreateButton(mainMenuPanel.transform, "QuitButton", "Quit", new Vector2(0, -50));

        // Create Lobby Panel (hidden by default)
        GameObject lobbyPanel = CreatePanel(canvasObj.transform, "LobbyPanel");
        lobbyPanel.SetActive(false);
        
        // Status Text
        TextMeshProUGUI statusText = CreateText(lobbyPanel.transform, "StatusText", "Finding match...", 24, new Vector2(0, 80));
        
        // Player Count Text
        TextMeshProUGUI playerCountText = CreateText(lobbyPanel.transform, "PlayerCountText", "Players: 0/2", 20, new Vector2(0, 30));
        
        // Cancel Button
        Button cancelBtn = CreateButton(lobbyPanel.transform, "CancelButton", "Cancel", new Vector2(0, -40));

        // Create MainMenuController and wire everything up
        GameObject controllerObj = new GameObject("MainMenuController");
        MainMenuController controller = controllerObj.AddComponent<MainMenuController>();
        
        SerializedObject controllerSO = new SerializedObject(controller);
        controllerSO.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
        controllerSO.FindProperty("lobbyPanel").objectReferenceValue = lobbyPanel;
        controllerSO.FindProperty("joinButton").objectReferenceValue = joinBtn;
        controllerSO.FindProperty("quitButton").objectReferenceValue = quitBtn;
        controllerSO.FindProperty("statusText").objectReferenceValue = statusText;
        controllerSO.FindProperty("playerCountText").objectReferenceValue = playerCountText;
        controllerSO.FindProperty("cancelButton").objectReferenceValue = cancelBtn;
        controllerSO.ApplyModifiedProperties();

        // Save scene
        string scenePath = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        
        Debug.Log("[MainMenuSetup] Created MainMenu scene at: " + scenePath);
        Debug.Log("[MainMenuSetup] Don't forget to add scenes to Build Settings!");
    }

    private static GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        return panel;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400, 60);
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return tmp;
    }

    private static Button CreateButton(Transform parent, string name, string text, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 50);
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.3f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return button;
    }

    private static void AddScenesToBuildSettings()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        
        // MainMenu first
        string mainMenuPath = "Assets/Scenes/MainMenu.unity";
        if (File.Exists(mainMenuPath))
        {
            scenes.Add(new EditorBuildSettingsScene(mainMenuPath, true));
            Debug.Log("[MainMenuSetup] Added MainMenu to build settings");
        }
        else
        {
            Debug.LogWarning("[MainMenuSetup] MainMenu.unity not found. Create it first!");
        }
        
        // DuelScreen second
        string duelScreenPath = "Assets/Scenes/DuelScreen.unity";
        if (File.Exists(duelScreenPath))
        {
            scenes.Add(new EditorBuildSettingsScene(duelScreenPath, true));
            Debug.Log("[MainMenuSetup] Added DuelScreen to build settings");
        }
        
        // Network Test (optional)
        string networkTestPath = "Assets/Scenes/Network Test.unity";
        if (File.Exists(networkTestPath))
        {
            scenes.Add(new EditorBuildSettingsScene(networkTestPath, true));
            Debug.Log("[MainMenuSetup] Added Network Test to build settings");
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[MainMenuSetup] Build settings updated!");
    }
}
#endif
