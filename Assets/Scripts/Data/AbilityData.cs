using UnityEngine;

namespace KOA.Data
{
    /// <summary>
    /// ScriptableObject that defines an ability's data.
    /// The behavior logic is determined by the behaviorType field which references
    /// a class inheriting from AbilityBehavior. Values like damage/heal/duration
    /// are conditionally used based on the behavior's RequiredFields.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbility", menuName = "KOA/Ability Data", order = 1)]
    public class AbilityData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this ability")]
        public string id;
        
        [Tooltip("Display name shown to players")]
        public string displayName;
        
        [TextArea(2, 4)]
        [Tooltip("Description shown on card. Use {damage}, {heal}, {duration} for dynamic values.")]
        public string description;

        [Header("Behavior")]
        [Tooltip("The type name of the AbilityBehavior class that executes this ability")]
        public string behaviorType;

        [Header("Effect Values (conditional based on behavior)")]
        [Tooltip("Damage dealt by this ability (used by Damage, Poison, BuffAttack, DrawCards behaviors)")]
        public int damage;
        
        [Tooltip("Amount healed by this ability (used by Heal behavior)")]
        public int healAmount;
        
        [Tooltip("Duration in turns for lasting effects (used by Poison, Stun, BuffAttack behaviors)")]
        public int duration;

        [Header("Targeting")]
        [Tooltip("What this ability can target")]
        public TargetType targetType = TargetType.SingleEnemy;

        [Header("Visual Effects")]
        [Tooltip("The prefab for visual effects during ability execution")]
        public GameObject effectPrefab;

        [Tooltip("Animation style when ability activates")]
        public AnimationType animationType = AnimationType.None;
        
        [Tooltip("VFX prefab to spawn during ability")]
        public GameObject vfxPrefab;
        
        [Tooltip("Icon displayed on the card")]
        public Sprite icon;

        /// <summary>
        /// Get the behavior instance for this ability.
        /// </summary>
        public Abilities.AbilityBehavior GetBehavior()
        {
            return Abilities.AbilityBehaviorRegistry.GetBehavior(behaviorType);
        }

        /// <summary>
        /// Execute this ability using its behavior.
        /// </summary>
        public void Execute(object source, object target, System.Action onComplete = null)
        {
            var behavior = GetBehavior();
            if (behavior == null)
            {
                Debug.LogWarning($"[AbilityData] No behavior found for type: {behaviorType}");
                onComplete?.Invoke();
                return;
            }

            var context = new Abilities.AbilityContext
            {
                Source = source,
                Target = target,
                AbilityData = this,
                OnComplete = onComplete
            };

            behavior.Execute(context);
        }

        /// <summary>
        /// Returns the description with placeholders replaced by actual values.
        /// </summary>
        public string GetFormattedDescription()
        {
            return description
                .Replace("{damage}", damage.ToString())
                .Replace("{heal}", healAmount.ToString())
                .Replace("{duration}", duration.ToString());
        }
    }
}
