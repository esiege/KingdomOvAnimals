using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Diagnostics;
using System.IO;

/// <summary>
/// Editor tools for quick multiplayer testing.
/// Builds the game and runs it alongside Play mode.
/// </summary>
public static class MultiplayerTestLauncher
{
    private const string BUILD_PATH = "Build/KingdomOvAnimals.exe";
    
    [MenuItem("Tools/Multiplayer Test/Build + Run Both (Host in Editor) %&b")]
    public static void BuildAndRunBoth()
    {
        // Build first
        if (!BuildGame())
        {
            UnityEngine.Debug.LogError("[MultiplayerTest] Build failed! Aborting.");
            return;
        }
        
        // Launch the build (this will be the Client)
        LaunchBuild();
        
        // Start Play mode in editor (this will be the Host)
        EditorApplication.isPlaying = true;
        
        UnityEngine.Debug.Log("[MultiplayerTest] Build launched as Client. Editor entering Play mode as Host.");
    }
    
    [MenuItem("Tools/Multiplayer Test/Run Build Only (No Play Mode)")]
    public static void RunBuildOnly()
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), BUILD_PATH);
        
        if (!File.Exists(fullPath))
        {
            UnityEngine.Debug.LogError($"[MultiplayerTest] Build not found at: {fullPath}\nUse 'Build Game' first.");
            return;
        }
        
        LaunchBuild();
        UnityEngine.Debug.Log("[MultiplayerTest] Build launched. Start Play mode manually to host.");
    }
    
    [MenuItem("Tools/Multiplayer Test/Build Game")]
    public static bool BuildGame()
    {
        UnityEngine.Debug.Log("[MultiplayerTest] Building game...");
        
        // Get scenes from build settings
        string[] scenes = GetBuildScenes();
        if (scenes.Length == 0)
        {
            UnityEngine.Debug.LogError("[MultiplayerTest] No scenes in Build Settings!");
            return false;
        }
        
        // Build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = BUILD_PATH,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        
        // Build
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log($"[MultiplayerTest] Build succeeded! Size: {report.summary.totalSize / (1024 * 1024)} MB");
            return true;
        }
        else
        {
            UnityEngine.Debug.LogError($"[MultiplayerTest] Build failed with {report.summary.totalErrors} errors.");
            return false;
        }
    }
    
    [MenuItem("Tools/Multiplayer Test/Open Build Folder")]
    public static void OpenBuildFolder()
    {
        string buildDir = Path.Combine(Directory.GetCurrentDirectory(), "Build");
        if (Directory.Exists(buildDir))
        {
            Process.Start("explorer.exe", buildDir);
        }
        else
        {
            UnityEngine.Debug.LogWarning("[MultiplayerTest] Build folder doesn't exist yet.");
        }
    }
    
    private static void LaunchBuild()
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), BUILD_PATH);
        
        if (!File.Exists(fullPath))
        {
            UnityEngine.Debug.LogError($"[MultiplayerTest] Executable not found: {fullPath}");
            return;
        }
        
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = fullPath,
            WorkingDirectory = Path.GetDirectoryName(fullPath),
            UseShellExecute = true
        };
        
        Process.Start(startInfo);
        UnityEngine.Debug.Log($"[MultiplayerTest] Launched: {fullPath}");
    }
    
    private static string[] GetBuildScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
            }
        }
        return scenes.ToArray();
    }
}
