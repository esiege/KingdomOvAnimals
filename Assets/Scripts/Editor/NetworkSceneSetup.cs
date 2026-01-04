#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using FishNet.Object;
using System.IO;

/// <summary>
/// Editor utility to set up FishNet networking components in the scene.
/// Access via menu: Tools > KingdomOvAnimals > Setup Network Scene
/// </summary>
public class NetworkSceneSetup : EditorWindow
{
    [MenuItem("Tools/KingdomOvAnimals/Setup Network Scene")]
    public static void ShowWindow()
    {
        GetWindow<NetworkSceneSetup>("Network Scene Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Network Scene Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This will set up all required networking components:\n" +
            "• NetworkPlayer prefab\n" +
            "• PlayerConnectionHandler in scene\n" +
            "• Register prefab with DefaultPrefabObjects",
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("Create NetworkPlayer Prefab", GUILayout.Height(30)))
        {
            CreateNetworkPlayerPrefab();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Add PlayerConnectionHandler to Scene", GUILayout.Height(30)))
        {
            AddPlayerConnectionHandler();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Setup Everything", GUILayout.Height(40)))
        {
            SetupEverything();
        }
    }

    private static void SetupEverything()
    {
        CreateNetworkPlayerPrefab();
        AddPlayerConnectionHandler();
        Debug.Log("[NetworkSceneSetup] Setup complete!");
    }

    private static void CreateNetworkPlayerPrefab()
    {
        // Ensure directory exists
        string prefabDir = "Assets/Prefabs/Network";
        if (!AssetDatabase.IsValidFolder(prefabDir))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            AssetDatabase.CreateFolder("Assets/Prefabs", "Network");
        }

        string prefabPath = $"{prefabDir}/NetworkPlayer.prefab";

        // Check if prefab already exists
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            Debug.Log("[NetworkSceneSetup] NetworkPlayer prefab already exists at: " + prefabPath);
            return;
        }

        // Create the prefab
        GameObject playerObj = new GameObject("NetworkPlayer");
        
        // Add required components
        NetworkObject networkObject = playerObj.AddComponent<NetworkObject>();
        NetworkPlayer networkPlayer = playerObj.AddComponent<NetworkPlayer>();

        // Save as prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(playerObj, prefabPath);
        
        // Clean up scene object
        DestroyImmediate(playerObj);

        // Register with DefaultPrefabObjects
        RegisterPrefabWithDefaultPrefabObjects(prefab);

        Debug.Log("[NetworkSceneSetup] Created NetworkPlayer prefab at: " + prefabPath);
    }

    private static void RegisterPrefabWithDefaultPrefabObjects(GameObject prefab)
    {
        // Find DefaultPrefabObjects asset
        string[] guids = AssetDatabase.FindAssets("t:DefaultPrefabObjects");
        if (guids.Length == 0)
        {
            Debug.LogWarning("[NetworkSceneSetup] DefaultPrefabObjects not found. You may need to add the prefab manually.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var defaultPrefabs = AssetDatabase.LoadAssetAtPath<FishNet.Managing.Object.DefaultPrefabObjects>(path);
        
        if (defaultPrefabs != null)
        {
            // Use reflection or the public API to add the prefab
            // DefaultPrefabObjects has a method to add prefabs
            SerializedObject serializedObject = new SerializedObject(defaultPrefabs);
            SerializedProperty prefabsProperty = serializedObject.FindProperty("_prefabs");
            
            // Check if already added
            NetworkObject nob = prefab.GetComponent<NetworkObject>();
            for (int i = 0; i < prefabsProperty.arraySize; i++)
            {
                var element = prefabsProperty.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == nob)
                {
                    Debug.Log("[NetworkSceneSetup] Prefab already registered with DefaultPrefabObjects");
                    return;
                }
            }

            // Add to array
            prefabsProperty.arraySize++;
            prefabsProperty.GetArrayElementAtIndex(prefabsProperty.arraySize - 1).objectReferenceValue = nob;
            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(defaultPrefabs);
            AssetDatabase.SaveAssets();
            
            Debug.Log("[NetworkSceneSetup] Registered prefab with DefaultPrefabObjects");
        }
    }

    private static void AddPlayerConnectionHandler()
    {
        // Check if already exists in scene
        PlayerConnectionHandler existing = FindObjectOfType<PlayerConnectionHandler>();
        if (existing != null)
        {
            Debug.Log("[NetworkSceneSetup] PlayerConnectionHandler already exists in scene");
            AssignPrefabToHandler(existing);
            return;
        }

        // Create new GameObject with handler
        GameObject handlerObj = new GameObject("PlayerConnectionHandler");
        PlayerConnectionHandler handler = handlerObj.AddComponent<PlayerConnectionHandler>();
        
        AssignPrefabToHandler(handler);
        
        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[NetworkSceneSetup] Added PlayerConnectionHandler to scene");
    }

    private static void AssignPrefabToHandler(PlayerConnectionHandler handler)
    {
        // Find and assign the NetworkPlayer prefab
        string prefabPath = "Assets/Prefabs/Network/NetworkPlayer.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            NetworkObject nob = prefab.GetComponent<NetworkObject>();
            SerializedObject serializedHandler = new SerializedObject(handler);
            SerializedProperty prefabProperty = serializedHandler.FindProperty("playerPrefab");
            prefabProperty.objectReferenceValue = nob;
            serializedHandler.ApplyModifiedProperties();
            
            Debug.Log("[NetworkSceneSetup] Assigned NetworkPlayer prefab to handler");
        }
        else
        {
            Debug.LogWarning("[NetworkSceneSetup] NetworkPlayer prefab not found. Create it first!");
        }
    }
}
#endif
