using UnityEngine;
using UnityEditor;
using KOA.Network;
using KOA.View;
using KOA.Migration;
using FishNet.Object;

namespace KOA.Editor
{
    /// <summary>
    /// Editor window to help set up and test the new Board State Architecture (Story 030).
    /// </summary>
    public class BoardStateSetupWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showSetupSection = true;
        private bool _showValidationSection = true;
        private bool _showDebugSection = true;
        
        [MenuItem("KOA/Board State Setup", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<BoardStateSetupWindow>("Board State Setup");
            window.minSize = new Vector2(400, 500);
        }
        
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            DrawHeader();
            EditorGUILayout.Space(10);
            
            DrawSetupSection();
            EditorGUILayout.Space(10);
            
            DrawValidationSection();
            EditorGUILayout.Space(10);
            
            DrawDebugSection();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Story 030: Board State Architecture", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool helps set up and validate the new board state architecture.\n" +
                "Follow the steps below to integrate the new system.",
                MessageType.Info);
        }
        
        #region Setup Section
        
        private void DrawSetupSection()
        {
            _showSetupSection = EditorGUILayout.Foldout(_showSetupSection, "Setup Steps", true, EditorStyles.foldoutHeader);
            if (!_showSetupSection) return;
            
            EditorGUI.indentLevel++;
            
            // Step 1: NetworkBoardState
            DrawSetupStep(
                "1. Add NetworkBoardState",
                "Creates the NetworkBoardState singleton for game state sync.",
                HasNetworkBoardState(),
                CreateNetworkBoardState
            );
            
            // Step 2: BoardView
            DrawSetupStep(
                "2. Add BoardView",
                "Creates the BoardView for rendering the board.",
                HasBoardView(),
                CreateBoardView
            );
            
            // Step 3: InputController
            DrawSetupStep(
                "3. Add InputController",
                "Creates the InputController for handling player input.",
                HasInputController(),
                CreateInputController
            );
            
            // Step 4: TurnUI
            DrawSetupStep(
                "4. Add TurnUI",
                "Creates the TurnUI for turn management display.",
                HasTurnUI(),
                CreateTurnUI
            );
            
            // Step 5: BoardStateBridge (optional)
            DrawSetupStep(
                "5. Add BoardStateBridge (Migration)",
                "Optional: Bridge between old and new systems during migration.",
                HasBoardStateBridge(),
                CreateBoardStateBridge
            );
            
            EditorGUILayout.Space(5);
            
            // Create All button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create All Missing Components", GUILayout.Width(220)))
            {
                CreateAllMissing();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawSetupStep(string title, string description, bool isComplete, System.Action onCreate)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            // Status icon
            var icon = isComplete ? EditorGUIUtility.IconContent("TestPassed") : EditorGUIUtility.IconContent("TestNormal");
            GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            
            GUI.enabled = !isComplete;
            if (GUILayout.Button("Create", GUILayout.Width(60)))
            {
                onCreate?.Invoke();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Validation Section
        
        private void DrawValidationSection()
        {
            _showValidationSection = EditorGUILayout.Foldout(_showValidationSection, "Validation", true, EditorStyles.foldoutHeader);
            if (!_showValidationSection) return;
            
            EditorGUI.indentLevel++;
            
            // Check CardLibrary
            var cardLibrary = FindObjectOfType<CardLibrary>();
            DrawValidationItem("CardLibrary", cardLibrary != null, 
                cardLibrary != null ? "Found in scene" : "Missing! Required for card data lookup.");
            
            // Check NetworkBoardState references
            var networkBoardState = FindObjectOfType<NetworkBoardState>();
            if (networkBoardState != null)
            {
                var so = new SerializedObject(networkBoardState);
                var cardLibRef = so.FindProperty("_cardLibrary");
                bool hasCardLib = cardLibRef != null && cardLibRef.objectReferenceValue != null;
                DrawValidationItem("NetworkBoardState.CardLibrary", hasCardLib,
                    hasCardLib ? "Assigned" : "Not assigned! Drag CardLibrary reference.");
            }
            
            // Check BoardView references
            var boardView = FindObjectOfType<BoardView>();
            if (boardView != null)
            {
                var so = new SerializedObject(boardView);
                var slotsRef = so.FindProperty("_playerSlots");
                bool hasSlots = slotsRef != null && slotsRef.arraySize >= 3;
                DrawValidationItem("BoardView.PlayerSlots", hasSlots,
                    hasSlots ? $"{slotsRef.arraySize} slots configured" : "Need 3 slot transforms!");
            }
            
            // Check for NetworkObject on NetworkBoardState
            if (networkBoardState != null)
            {
                var networkObj = networkBoardState.GetComponent<NetworkObject>();
                DrawValidationItem("NetworkBoardState.NetworkObject", networkObj != null,
                    networkObj != null ? "Has NetworkObject" : "Missing NetworkObject component!");
            }
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh Validation", GUILayout.Width(150)))
            {
                Repaint();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawValidationItem(string name, bool isValid, string message)
        {
            EditorGUILayout.BeginHorizontal();
            
            var icon = isValid 
                ? EditorGUIUtility.IconContent("TestPassed") 
                : EditorGUIUtility.IconContent("TestFailed");
            GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(18));
            
            EditorGUILayout.LabelField(name, GUILayout.Width(180));
            EditorGUILayout.LabelField(message, isValid ? EditorStyles.miniLabel : EditorStyles.miniBoldLabel);
            
            EditorGUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Debug Section
        
        private void DrawDebugSection()
        {
            _showDebugSection = EditorGUILayout.Foldout(_showDebugSection, "Runtime Debug", true, EditorStyles.foldoutHeader);
            if (!_showDebugSection) return;
            
            EditorGUI.indentLevel++;
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see runtime debug info.", MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }
            
            // Show current board state
            if (NetworkBoardState.Instance != null)
            {
                var state = NetworkBoardState.Instance.GetState();
                if (state != null)
                {
                    EditorGUILayout.LabelField($"Turn: {state.TurnNumber}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Active Player: {state.CurrentTurnPlayerId}");
                    EditorGUILayout.LabelField($"Game Active: {state.IsGameActive}, Winner: {state.WinnerPlayerId}");
                    
                    EditorGUILayout.Space(5);
                    
                    for (int p = 0; p < 2; p++)
                    {
                        var player = state.GetPlayer(p);
                        EditorGUILayout.LabelField($"Player {p}:", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"  Health: {player.Health}/{player.MaxHealth}");
                        EditorGUILayout.LabelField($"  Mana: {player.Mana}/{player.MaxMana}");
                        EditorGUILayout.LabelField($"  Hand: {player.Hand.Count} cards");
                        EditorGUILayout.LabelField($"  Deck: {player.Deck.Count} cards");
                        
                        // Board slots
                        string boardStr = "";
                        for (int s = 0; s < 3; s++)
                        {
                            var card = player.GetCardInSlot(s);
                            boardStr += card.IsValid ? $"[{card.CardDataId}] " : "[empty] ";
                        }
                        EditorGUILayout.LabelField($"  Board: {boardStr}");
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("BoardState is null");
                }
            }
            else
            {
                EditorGUILayout.LabelField("NetworkBoardState.Instance is null");
            }
            
            EditorGUILayout.Space(5);
            
            // Debug buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Log State"))
            {
                LogCurrentState();
            }
            if (GUILayout.Button("Force Refresh Views"))
            {
                ForceRefreshViews();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        #endregion
        
        #region Component Checks
        
        private bool HasNetworkBoardState() => FindObjectOfType<NetworkBoardState>() != null;
        private bool HasBoardView() => FindObjectOfType<BoardView>() != null;
        private bool HasInputController() => FindObjectOfType<InputController>() != null;
        private bool HasTurnUI() => FindObjectOfType<TurnUI>() != null;
        private bool HasBoardStateBridge() => FindObjectOfType<BoardStateBridge>() != null;
        
        #endregion
        
        #region Component Creation
        
        private void CreateNetworkBoardState()
        {
            var go = new GameObject("NetworkBoardState");
            go.AddComponent<NetworkBoardState>();
            go.AddComponent<NetworkObject>();
            
            // Try to assign CardLibrary
            var cardLib = FindObjectOfType<CardLibrary>();
            if (cardLib != null)
            {
                var so = new SerializedObject(go.GetComponent<NetworkBoardState>());
                var prop = so.FindProperty("_cardLibrary");
                if (prop != null)
                {
                    prop.objectReferenceValue = cardLib;
                    so.ApplyModifiedProperties();
                }
            }
            
            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create NetworkBoardState");
            Debug.Log("[BoardStateSetup] Created NetworkBoardState");
        }
        
        private void CreateBoardView()
        {
            var go = new GameObject("BoardView");
            go.AddComponent<BoardView>();
            
            // Create slot containers
            var playerSlotsParent = new GameObject("PlayerSlots");
            playerSlotsParent.transform.SetParent(go.transform);
            
            var opponentSlotsParent = new GameObject("OpponentSlots");
            opponentSlotsParent.transform.SetParent(go.transform);
            
            // Create 3 slots for each player
            var playerSlots = new Transform[3];
            var opponentSlots = new Transform[3];
            
            for (int i = 0; i < 3; i++)
            {
                var pSlot = new GameObject($"Slot_{i}");
                pSlot.transform.SetParent(playerSlotsParent.transform);
                pSlot.transform.localPosition = new Vector3((i - 1) * 2f, 0, 0);
                playerSlots[i] = pSlot.transform;
                
                var oSlot = new GameObject($"Slot_{i}");
                oSlot.transform.SetParent(opponentSlotsParent.transform);
                oSlot.transform.localPosition = new Vector3((i - 1) * 2f, 0, 0);
                opponentSlots[i] = oSlot.transform;
            }
            
            playerSlotsParent.transform.localPosition = new Vector3(0, -2, 0);
            opponentSlotsParent.transform.localPosition = new Vector3(0, 2, 0);
            
            // Assign slots to BoardView
            var so = new SerializedObject(go.GetComponent<BoardView>());
            
            var playerSlotsProp = so.FindProperty("_playerSlots");
            if (playerSlotsProp != null)
            {
                playerSlotsProp.arraySize = 3;
                for (int i = 0; i < 3; i++)
                {
                    playerSlotsProp.GetArrayElementAtIndex(i).objectReferenceValue = playerSlots[i];
                }
            }
            
            var opponentSlotsProp = so.FindProperty("_opponentSlots");
            if (opponentSlotsProp != null)
            {
                opponentSlotsProp.arraySize = 3;
                for (int i = 0; i < 3; i++)
                {
                    opponentSlotsProp.GetArrayElementAtIndex(i).objectReferenceValue = opponentSlots[i];
                }
            }
            
            so.ApplyModifiedProperties();
            
            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create BoardView");
            Debug.Log("[BoardStateSetup] Created BoardView with slot containers");
        }
        
        private void CreateInputController()
        {
            var go = new GameObject("InputController");
            var ic = go.AddComponent<InputController>();
            
            // Try to assign BoardView
            var boardView = FindObjectOfType<BoardView>();
            if (boardView != null)
            {
                var so = new SerializedObject(ic);
                var prop = so.FindProperty("_boardView");
                if (prop != null)
                {
                    prop.objectReferenceValue = boardView;
                    so.ApplyModifiedProperties();
                }
            }
            
            // Try to assign CardLibrary
            var cardLib = FindObjectOfType<CardLibrary>();
            if (cardLib != null)
            {
                var so = new SerializedObject(ic);
                var prop = so.FindProperty("_cardLibrary");
                if (prop != null)
                {
                    prop.objectReferenceValue = cardLib;
                    so.ApplyModifiedProperties();
                }
            }
            
            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create InputController");
            Debug.Log("[BoardStateSetup] Created InputController");
        }
        
        private void CreateTurnUI()
        {
            var go = new GameObject("TurnUI");
            go.AddComponent<TurnUI>();
            
            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create TurnUI");
            Debug.Log("[BoardStateSetup] Created TurnUI (configure UI references manually)");
        }
        
        private void CreateBoardStateBridge()
        {
            var go = new GameObject("BoardStateBridge");
            go.AddComponent<BoardStateBridge>();
            go.AddComponent<NetworkObject>();
            
            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create BoardStateBridge");
            Debug.Log("[BoardStateSetup] Created BoardStateBridge");
        }
        
        private void CreateAllMissing()
        {
            if (!HasNetworkBoardState()) CreateNetworkBoardState();
            if (!HasBoardView()) CreateBoardView();
            if (!HasInputController()) CreateInputController();
            if (!HasTurnUI()) CreateTurnUI();
            
            Debug.Log("[BoardStateSetup] Created all missing components");
        }
        
        #endregion
        
        #region Debug Actions
        
        private void LogCurrentState()
        {
            if (NetworkBoardState.Instance == null)
            {
                Debug.Log("[BoardStateSetup] NetworkBoardState.Instance is null");
                return;
            }
            
            var state = NetworkBoardState.Instance.GetState();
            if (state == null)
            {
                Debug.Log("[BoardStateSetup] BoardState is null");
                return;
            }
            
            Debug.Log($"[BoardStateSetup] === BOARD STATE ===\n{state}");
        }
        
        private void ForceRefreshViews()
        {
            var boardView = FindObjectOfType<BoardView>();
            if (boardView != null && NetworkBoardState.Instance != null)
            {
                var state = NetworkBoardState.Instance.GetState();
                // Call RenderBoard via reflection or make it public
                Debug.Log("[BoardStateSetup] Would refresh BoardView (make RenderBoard public to enable)");
            }
        }
        
        #endregion
    }
}
