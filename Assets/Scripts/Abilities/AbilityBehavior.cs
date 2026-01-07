using UnityEngine;

namespace KOA.Abilities
{
    /// <summary>
    /// Base class for ability behaviors. Defines what an ability does when executed.
    /// Concrete implementations define specific logic (damage, heal, return to hand, etc.)
    /// </summary>
    public abstract class AbilityBehavior
    {
        /// <summary>
        /// Display name shown in the editor dropdown.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Fields required by this behavior. Used to show/hide conditional fields in the editor.
        /// Valid values: "damage", "healAmount", "duration"
        /// </summary>
        public abstract string[] RequiredFields { get; }

        /// <summary>
        /// Execute the ability logic.
        /// </summary>
        /// <param name="context">Context containing source, target, and ability data</param>
        public abstract void Execute(AbilityContext context);
    }

    /// <summary>
    /// Context passed to ability behaviors during execution.
    /// </summary>
    public class AbilityContext
    {
        /// <summary>
        /// The card using this ability.
        /// </summary>
        public object Source { get; set; }

        /// <summary>
        /// The target of the ability (can be card, player, etc.)
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// The ability data containing values (damage, heal, duration, etc.)
        /// </summary>
        public Data.AbilityData AbilityData { get; set; }

        /// <summary>
        /// Optional callback when ability completes.
        /// </summary>
        public System.Action OnComplete { get; set; }
    }
}
