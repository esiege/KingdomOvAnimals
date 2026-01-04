using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor tool to set up the DuelScreen scene for networking.
/// Adds NetworkGameManager and links PlayerController references.
/// Accessed via Tools > KingdomOvAnimals > Setup DuelScreen Networking
/// </summary>
public class DuelSceneNetworkSetup : EditorWindow
{
    [MenuItem("Tools/KingdomOvAnimals/Setup DuelScreen Networking")]
    public static void ShowWindow()
    {
        GetWindow<DuelSceneNetworkSetup>("DuelScreen Network Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("DuelScreen Network Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This tool sets up the DuelScreen scene for networked play.\n\n" +
            "It will:\n" +
            "1. Add NetworkGameManager to the scene\n" +
            "2. Link PlayerController references\n" +
            "3. Add NetworkObject to the manager\n\n" +
            "Make sure you have the DuelScreen scene open!",
            MessageType.Info);

        GUILayout.Space(10);

        // Check current scene
        string sceneName = EditorSceneManager.GetActiveScene().name;
        EditorGUILayout.LabelField("Current Scene:", sceneName);

        if (sceneName != "DuelScreen")
        {
            EditorGUILayout.HelpBox("Please open the DuelScreen scene first!", MessageType.Warning);
            
            if (GUILayout.Button("Open DuelScreen Scene"))
            {
                // Try to find and open DuelScreen
                string[] guids = AssetDatabase.FindAssets("DuelScreen t:Scene");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    EditorSceneManager.OpenScene(path);
                }
            }
            return;
        }

        GUILayout.Space(10);

        // Find existing components
        NetworkGameManager existingManager = FindObjectOfType<NetworkGameManager>();
        PlayerController[] playerControllers = FindObjectsOfType<PlayerController>();

        GUILayout.Label("Scene Status:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        
        // NetworkGameManager status
        if (existingManager != null)
        {
            EditorGUILayout.LabelField("✅ NetworkGameManager found");
            EditorGUILayout.ObjectField("Manager:", existingManager, typeof(NetworkGameManager), true);
        }
        else
        {
            EditorGUILayout.LabelField("❌ NetworkGameManager not found");
        }

        // PlayerControllers status
        EditorGUILayout.LabelField($"Found {playerControllers.Length} PlayerController(s):");
        foreach (var pc in playerControllers)
        {
            EditorGUILayout.ObjectField(pc.gameObject.name, pc, typeof(PlayerController), true);
        }
        
        EditorGUILayout.EndVertical();

        GUILayout.Space(20);

        if (GUILayout.Button("Setup DuelScreen for Networking", GUILayout.Height(30)))
        {
            SetupDuelScreenNetworking(playerControllers);
        }

        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "After setup:\n" +
            "1. Save the scene (Ctrl+S)\n" +
            "2. Remove NetworkHudCanvas if present\n" +
            "3. Test with 2 players from MainMenu",
            MessageType.None);
    }

    private void SetupDuelScreenNetworking(PlayerController[] playerControllers)
    {
        // Find or create NetworkGameManager
        NetworkGameManager manager = FindObjectOfType<NetworkGameManager>();
        
        if (manager == null)
        {
            // Create new GameObject with NetworkGameManager
            GameObject managerObj = new GameObject("NetworkGameManager");
            manager = managerObj.AddComponent<NetworkGameManager>();
            
            // Add NetworkObject (required for NetworkBehaviour)
            managerObj.AddComponent<FishNet.Object.NetworkObject>();
            
            Debug.Log("[Setup] Created NetworkGameManager");
        }

        // Find player and opponent controllers
        PlayerController localPlayer = null;
        PlayerController opponent = null;

        foreach (var pc in playerControllers)
        {
            string name = pc.gameObject.name.ToLower();
            if (name.Contains("opponent") || name.Contains("enemy"))
            {
                opponent = pc;
            }
            else if (name.Contains("player"))
            {
                localPlayer = pc;
            }
        }

        // If we couldn't determine by name, use first two found
        if (localPlayer == null && playerControllers.Length > 0)
        {
            localPlayer = playerControllers[0];
        }
        if (opponent == null && playerControllers.Length > 1)
        {
            opponent = playerControllers[1];
        }

        // Assign references
        manager.localPlayerController = localPlayer;
        manager.opponentPlayerController = opponent;

        Debug.Log($"[Setup] Assigned localPlayerController: {(localPlayer != null ? localPlayer.gameObject.name : "null")}");
        Debug.Log($"[Setup] Assigned opponentPlayerController: {(opponent != null ? opponent.gameObject.name : "null")}");

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        EditorUtility.DisplayDialog("Setup Complete", 
            "NetworkGameManager has been set up!\n\n" +
            "Please verify the PlayerController assignments in the Inspector,\n" +
            "then save the scene.", 
            "OK");
    }
}
