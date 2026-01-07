using UnityEngine;

namespace KOA.Abilities.Behaviors
{
    /// <summary>
    /// Buff attack ability - increases target's damage for a duration.
    /// </summary>
    public class BuffAttackAbility : AbilityBehavior
    {
        public override string DisplayName => "Buff Attack";
        
        public override string[] RequiredFields => new[] { "damage", "duration" }; // damage = buff amount

        public override void Execute(AbilityContext context)
        {
            int buffAmount = context.AbilityData.damage; // Reusing damage field for buff amount
            int duration = context.AbilityData.duration;
            Debug.Log($"[BuffAttackAbility] Buffing target attack by +{buffAmount} for {duration} turns");
            
            // TODO: Implement actual buff logic when status effect system is ready
            // Example: ((CardInstance)context.Target).ApplyStatusEffect(new AttackBuffEffect(buffAmount, duration));
            
            context.OnComplete?.Invoke();
        }
    }
}
