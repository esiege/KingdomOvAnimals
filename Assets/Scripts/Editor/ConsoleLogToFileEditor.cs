using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utilities for ConsoleLogToFile
/// </summary>
public static class ConsoleLogToFileEditor
{
    private const string OBJECT_NAME = "LogCapture";
    
    [MenuItem("Tools/Log Capture/Add to Scene")]
    public static void AddToScene()
    {
        // Check if already exists
        var existing = Object.FindObjectOfType<ConsoleLogToFile>();
        if (existing != null)
        {
            Debug.Log($"[LogCapture] Already exists on '{existing.gameObject.name}'");
            Selection.activeGameObject = existing.gameObject;
            return;
        }
        
        // Create new GameObject
        var go = new GameObject(OBJECT_NAME);
        go.AddComponent<ConsoleLogToFile>();
        
        // Register for undo
        Undo.RegisterCreatedObjectUndo(go, "Add Log Capture");
        
        Selection.activeGameObject = go;
        Debug.Log("[LogCapture] Added to scene. Log will save to Docs/log.txt on play.");
    }
    
    [MenuItem("Tools/Log Capture/Open Editor Log")]
    public static void OpenLogFile()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "..", "Logs/GameLogs/log_editor.txt");
        if (System.IO.File.Exists(path))
        {
            EditorUtility.OpenWithDefaultApp(path);
        }
        else
        {
            Debug.LogWarning("[LogCapture] Editor log not found. Run Play mode first to generate it.");
        }
    }
    
    [MenuItem("Tools/Log Capture/Open Build Log")]
    public static void OpenClientLogFile()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "..", "Logs/GameLogs/log_build.txt");
        if (System.IO.File.Exists(path))
        {
            EditorUtility.OpenWithDefaultApp(path);
        }
        else
        {
            Debug.LogWarning("[LogCapture] Build log not found. Run a build first to generate it.");
        }
    }
    
    [MenuItem("Tools/Log Capture/Open Logs Folder")]
    public static void OpenLogsFolder()
    {
        string logsDir = System.IO.Path.Combine(Application.dataPath, "..", "Logs/GameLogs");
        if (System.IO.Directory.Exists(logsDir))
        {
            System.Diagnostics.Process.Start("explorer.exe", logsDir);
        }
        else
        {
            Debug.LogWarning("[LogCapture] Logs folder doesn't exist yet. Run the game first.");
        }
    }
    
    [MenuItem("Tools/Log Capture/Clear Editor Log")]
    public static void ClearLogFile()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "..", "Logs/GameLogs/log_editor.txt");
        if (System.IO.File.Exists(path))
        {
            System.IO.File.WriteAllText(path, $"=== Log cleared: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
            Debug.Log("[LogCapture] Editor log cleared.");
        }
        else
        {
            Debug.LogWarning("[LogCapture] Editor log not found.");
        }
    }
}
