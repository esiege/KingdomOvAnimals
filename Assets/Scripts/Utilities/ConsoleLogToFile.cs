using UnityEngine;
using System.IO;
using System;

/// <summary>
/// Captures all Unity console output to a file. Clears file on each play.
/// Attach to a GameObject that persists (or use DontDestroyOnLoad).
/// 
/// IMPORTANT: Editor and Build write to DIFFERENT log files!
/// - Editor (Host): Docs/log.txt
/// - Build (Client): Docs/log_client.txt
/// </summary>
public class ConsoleLogToFile : MonoBehaviour
{
    [Tooltip("Base path relative to project root. Filename will be replaced with 'log_editor.txt' or 'log_build.txt'")]
    public string logFilePath = "Logs/GameLogs/log.txt";
    
    [Tooltip("Include stack traces in log file")]
    public bool includeStackTrace = true;
    
    [Tooltip("Include timestamp on each line")]
    public bool includeTimestamp = true;
    
    private static ConsoleLogToFile instance;
    private StreamWriter logWriter;
    private string fullPath;
    private DateTime startTime;
    
    void Awake()
    {
        // Singleton pattern
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        startTime = DateTime.Now;
        
        // Determine if we're in editor or build
        bool isEditor = Application.isEditor;
        
        // Set filename based on editor vs build
        string dir = Path.GetDirectoryName(logFilePath);
        string filename = isEditor ? "log_editor.txt" : "log_build.txt";
        string basePath = Path.Combine(dir ?? "", filename);
        
        // Resolve path - use different base for editor vs build
        if (Path.IsPathRooted(basePath))
        {
            fullPath = basePath;
        }
        else if (isEditor)
        {
            // Editor: relative to project root
            fullPath = Path.Combine(Application.dataPath, "..", basePath);
        }
        else
        {
            // Build: write next to executable so it's in the same project folder
            // Application.dataPath in build = <path>/KingdomOvAnimals_Data
            // We want to go up two levels to get to project root: Build/../ = project root
            fullPath = Path.Combine(Application.dataPath, "..", "..", basePath);
        }
        
        // Ensure directory exists
        string directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        // Clear and open file
        logWriter = new StreamWriter(fullPath, false); // false = overwrite
        logWriter.AutoFlush = true;
        
        // Write header
        string mode = isEditor ? "EDITOR/HOST" : "BUILD/CLIENT";
        logWriter.WriteLine($"=== Log started: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ({mode}) ===");
        logWriter.WriteLine($"=== Log file: {fullPath} ===");
        logWriter.WriteLine();
        
        // Hook into Unity's log system
        Application.logMessageReceived += HandleLog;
    }
    
    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
        
        if (logWriter != null)
        {
            logWriter.WriteLine();
            logWriter.WriteLine($"=== Log ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            logWriter.Close();
            logWriter = null;
        }
    }
    
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (logWriter == null) return;
        
        string prefix = type switch
        {
            LogType.Error => "[ERROR] ",
            LogType.Warning => "[WARN] ",
            LogType.Exception => "[EXCEPTION] ",
            LogType.Assert => "[ASSERT] ",
            _ => ""
        };
        
        // Add timestamp if enabled
        string timestamp = "";
        if (includeTimestamp)
        {
            var elapsed = DateTime.Now - startTime;
            timestamp = $"[{elapsed.TotalSeconds:F2}s] ";
        }
        
        logWriter.WriteLine($"{timestamp}{prefix}{logString}");
        
        if (includeStackTrace && !string.IsNullOrEmpty(stackTrace))
        {
            logWriter.WriteLine(stackTrace);
        }
    }
    
    /// <summary>
    /// Manually flush the log file
    /// </summary>
    public static void Flush()
    {
        instance?.logWriter?.Flush();
    }
}
