using UnityEngine;

namespace KOA.Data
{
    /// <summary>
    /// ScriptableObject that defines an ability's data.
    /// The behavior (code) lives in effect prefabs; this is just the data.
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

        [Header("Cost")]
        [Tooltip("Mana cost to use this ability")]
        public int manaCost;

        [Header("Effect Values")]
        [Tooltip("Damage dealt by this ability (if applicable)")]
        public int damage;
        
        [Tooltip("Amount healed by this ability (if applicable)")]
        public int healAmount;
        
        [Tooltip("Duration in turns for lasting effects")]
        public int duration;

        [Header("Targeting")]
        [Tooltip("What this ability can target")]
        public TargetType targetType = TargetType.SingleEnemy;

        [Header("Behavior")]
        [Tooltip("The prefab containing the effect logic (AbilityEffect component)")]
        public GameObject effectPrefab;

        [Header("Visuals")]
        [Tooltip("Animation style when ability activates")]
        public AnimationType animationType = AnimationType.None;
        
        [Tooltip("VFX prefab to spawn during ability")]
        public GameObject vfxPrefab;
        
        [Tooltip("Icon displayed on the card")]
        public Sprite icon;

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
