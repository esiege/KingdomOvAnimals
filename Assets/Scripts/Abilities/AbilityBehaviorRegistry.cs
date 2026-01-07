using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KOA.Abilities
{
    /// <summary>
    /// Registry that discovers and manages all AbilityBehavior types via reflection.
    /// Provides lookup by type name for serialization and dropdown population.
    /// </summary>
    public static class AbilityBehaviorRegistry
    {
        private static Dictionary<string, Type> _behaviorTypes;
        private static Dictionary<string, AbilityBehavior> _behaviorInstances;
        private static bool _initialized = false;

        /// <summary>
        /// All discovered behavior type names (for dropdown population).
        /// </summary>
        public static IReadOnlyList<string> BehaviorTypeNames
        {
            get
            {
                EnsureInitialized();
                return _behaviorTypes.Keys.ToList();
            }
        }

        /// <summary>
        /// All discovered behavior display names (for UI).
        /// </summary>
        public static IReadOnlyList<string> BehaviorDisplayNames
        {
            get
            {
                EnsureInitialized();
                return _behaviorInstances.Values.Select(b => b.DisplayName).ToList();
            }
        }

        /// <summary>
        /// Get a behavior instance by its type name.
        /// </summary>
        public static AbilityBehavior GetBehavior(string typeName)
        {
            EnsureInitialized();
            
            if (string.IsNullOrEmpty(typeName))
                return null;
                
            return _behaviorInstances.TryGetValue(typeName, out var behavior) ? behavior : null;
        }

        /// <summary>
        /// Get the type name for a behavior by its display name.
        /// </summary>
        public static string GetTypeNameFromDisplayName(string displayName)
        {
            EnsureInitialized();
            
            foreach (var kvp in _behaviorInstances)
            {
                if (kvp.Value.DisplayName == displayName)
                    return kvp.Key;
            }
            return null;
        }

        /// <summary>
        /// Get the display name for a behavior by its type name.
        /// </summary>
        public static string GetDisplayNameFromTypeName(string typeName)
        {
            var behavior = GetBehavior(typeName);
            return behavior?.DisplayName ?? "(None)";
        }

        /// <summary>
        /// Get the required fields for a behavior by its type name.
        /// </summary>
        public static string[] GetRequiredFields(string typeName)
        {
            var behavior = GetBehavior(typeName);
            return behavior?.RequiredFields ?? new string[0];
        }

        /// <summary>
        /// Force re-initialization (useful after code changes in editor).
        /// </summary>
        public static void Refresh()
        {
            _initialized = false;
            EnsureInitialized();
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            
            _behaviorTypes = new Dictionary<string, Type>();
            _behaviorInstances = new Dictionary<string, AbilityBehavior>();
            
            // Find all types that inherit from AbilityBehavior
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && typeof(AbilityBehavior).IsAssignableFrom(t));
                    
                    foreach (var type in types)
                    {
                        try
                        {
                            var instance = (AbilityBehavior)Activator.CreateInstance(type);
                            _behaviorTypes[type.Name] = type;
                            _behaviorInstances[type.Name] = instance;
                            Debug.Log($"[AbilityBehaviorRegistry] Registered: {type.Name} ({instance.DisplayName})");
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[AbilityBehaviorRegistry] Failed to instantiate {type.Name}: {e.Message}");
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that can't be loaded
                }
            }
            
            Debug.Log($"[AbilityBehaviorRegistry] Initialized with {_behaviorTypes.Count} behaviors");
            _initialized = true;
        }
    }
}
