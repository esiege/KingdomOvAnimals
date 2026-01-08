using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using KOA.Data;

/// <summary>
/// Editor utility to set up the Data-Driven Card Spawning system (Story 026).
/// Accessible via menu: Tools > KingdomOvAnimals > Setup Data-Driven Cards
/// </summary>
public class DataDrivenCardSetup : EditorWindow
{
    private GameObject cardPrefab;
    private List<CardData> cardDataAssets = new List<CardData>();
    private bool autoFindAssets = true;
    
    [MenuItem("Tools/KingdomOvAnimals/Setup Data-Driven Cards")]
    public static void ShowWindow()
    {
        var window = GetWindow<DataDrivenCardSetup>("Card Setup");
        window.minSize = new Vector2(400, 300);
        window.LoadDefaults();
    }
    
    private void LoadDefaults()
    {
        // Try to find Card.prefab
        string[] guids = AssetDatabase.FindAssets("t:Prefab Card", new[] { "Assets/Prefabs" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith("Card.prefab"))
            {
                cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                break;
            }
        }
        
        // Load all CardData from Resources/Cards
        LoadCardDataAssets();
    }
    
    private void LoadCardDataAssets()
    {
        cardDataAssets.Clear();
        var loaded = Resources.LoadAll<CardData>("Cards");
        cardDataAssets.AddRange(loaded);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Data-Driven Card Setup (Story 026)", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // Card Prefab
        EditorGUILayout.LabelField("Card Prefab", EditorStyles.boldLabel);
        cardPrefab = (GameObject)EditorGUILayout.ObjectField("Card.prefab", cardPrefab, typeof(GameObject), false);
        
        if (cardPrefab == null)
        {
            EditorGUILayout.HelpBox("Card.prefab not found! Create one in Assets/Prefabs/Card/", MessageType.Warning);
        }
        else if (cardPrefab.GetComponent<CardController>() == null)
        {
            EditorGUILayout.HelpBox("Card.prefab doesn't have CardController!", MessageType.Error);
        }
        else
        {
            EditorGUILayout.HelpBox("Card.prefab found ✓", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        // CardData Assets
        EditorGUILayout.LabelField($"CardData Assets ({cardDataAssets.Count} found)", EditorStyles.boldLabel);
        
        if (cardDataAssets.Count == 0)
        {
            EditorGUILayout.HelpBox("No CardData assets found in Resources/Cards/", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var card in cardDataAssets)
            {
                EditorGUILayout.LabelField($"  • {card.displayName} (HP:{card.health}, Mana:{card.manaCost})");
            }
            EditorGUILayout.EndVertical();
        }
        
        if (GUILayout.Button("Refresh CardData"))
        {
            LoadCardDataAssets();
        }
        
        GUILayout.Space(20);
        
        // Setup Button
        EditorGUI.BeginDisabledGroup(cardPrefab == null || cardDataAssets.Count == 0);
        
        if (GUILayout.Button("Setup Current Scene", GUILayout.Height(40)))
        {
            SetupCurrentScene();
        }
        
        EditorGUI.EndDisabledGroup();
        
        GUILayout.Space(10);
        
        // Status
        EditorGUILayout.LabelField("Scene Status", EditorStyles.boldLabel);
        CheckSceneStatus();
    }
    
    private void CheckSceneStatus()
    {
        var encounterController = FindObjectOfType<EncounterController>();
        var playerControllers = FindObjectsOfType<PlayerController>();
        
        if (encounterController == null)
        {
            EditorGUILayout.HelpBox("EncounterController not found in scene", MessageType.Warning);
            return;
        }
        
        // Check cardPrefab assignment
        var cardPrefabField = encounterController.cardPrefab;
        if (cardPrefabField != null)
        {
            EditorGUILayout.LabelField("  EncounterController.cardPrefab: ✓ Assigned");
        }
        else
        {
            EditorGUILayout.LabelField("  EncounterController.cardPrefab: ✗ Not assigned");
        }
        
        // Check player decks
        foreach (var pc in playerControllers)
        {
            string deckStatus = pc.deck != null && pc.deck.Count > 0 
                ? $"✓ {pc.deck.Count} CardData" 
                : "✗ Empty";
            EditorGUILayout.LabelField($"  {pc.gameObject.name}.deck: {deckStatus}");
        }
    }
    
    private void SetupCurrentScene()
    {
        Undo.SetCurrentGroupName("Setup Data-Driven Cards");
        int undoGroup = Undo.GetCurrentGroup();
        
        // Find EncounterController
        var encounterController = FindObjectOfType<EncounterController>();
        if (encounterController == null)
        {
            EditorUtility.DisplayDialog("Setup Failed", "EncounterController not found in scene!", "OK");
            return;
        }
        
        // Assign card prefab
        Undo.RecordObject(encounterController, "Assign Card Prefab");
        encounterController.cardPrefab = cardPrefab;
        EditorUtility.SetDirty(encounterController);
        
        // Find PlayerControllers
        var playerControllers = FindObjectsOfType<PlayerController>();
        
        foreach (var pc in playerControllers)
        {
            Undo.RecordObject(pc, "Setup Player Deck");
            
            // Clear existing deck
            pc.deck.Clear();
            
            // Add all CardData assets
            foreach (var cardData in cardDataAssets)
            {
                pc.deck.Add(cardData);
            }
            
            EditorUtility.SetDirty(pc);
            Debug.Log($"[Setup] Added {cardDataAssets.Count} cards to {pc.gameObject.name}'s deck");
        }
        
        Undo.CollapseUndoOperations(undoGroup);
        
        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        EditorUtility.DisplayDialog("Setup Complete", 
            $"✓ Card prefab assigned to EncounterController\n" +
            $"✓ {cardDataAssets.Count} CardData added to {playerControllers.Length} player deck(s)\n\n" +
            "Save the scene and enter Play mode to test!", 
            "OK");
    }
}
