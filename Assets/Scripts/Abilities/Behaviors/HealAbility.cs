using UnityEngine;
using KOA.Abilities;

namespace KOA.Abilities.Behaviors
{
    /// <summary>
    /// Heal ability - restores health to the target.
    /// </summary>
    public class HealAbility : AbilityBehavior
    {
        public override string DisplayName => "Heal";
        
        public override string[] RequiredFields => new[] { "healAmount" };

        public override void Execute(AbilityContext context)
        {
            int heal = context.AbilityData.healAmount;
            Debug.Log($"[HealAbility] Healing target for {heal}");
            
            // TODO: Implement actual heal logic when CardInstance is ready
            // Example: ((CardInstance)context.Target).Heal(heal);
            
            context.OnComplete?.Invoke();
        }
    }
}
