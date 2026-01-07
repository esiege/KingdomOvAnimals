using UnityEngine;

namespace KOA.Abilities.Behaviors
{
    /// <summary>
    /// Poison ability - applies damage over time effect.
    /// </summary>
    public class PoisonAbility : AbilityBehavior
    {
        public override string DisplayName => "Poison (DoT)";
        
        public override string[] RequiredFields => new[] { "damage", "duration" };

        public override void Execute(AbilityContext context)
        {
            int damage = context.AbilityData.damage;
            int duration = context.AbilityData.duration;
            Debug.Log($"[PoisonAbility] Applying poison: {damage} damage for {duration} turns");
            
            // TODO: Implement actual poison logic when status effect system is ready
            // Example: ((CardInstance)context.Target).ApplyStatusEffect(new PoisonEffect(damage, duration));
            
            context.OnComplete?.Invoke();
        }
    }
}
