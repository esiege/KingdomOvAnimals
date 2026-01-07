using UnityEngine;

namespace KOA.Abilities.Behaviors
{
    /// <summary>
    /// Simple damage ability - deals damage to the target.
    /// </summary>
    public class DamageAbility : AbilityBehavior
    {
        public override string DisplayName => "Damage";
        
        public override string[] RequiredFields => new[] { "damage" };

        public override void Execute(AbilityContext context)
        {
            int damage = context.AbilityData.damage;
            Debug.Log($"[DamageAbility] Dealing {damage} damage to target");
            
            // TODO: Implement actual damage logic when CardInstance is ready
            // Example: ((CardInstance)context.Target).TakeDamage(damage);
            
            context.OnComplete?.Invoke();
        }
    }
}
