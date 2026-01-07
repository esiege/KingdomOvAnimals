using UnityEngine;

namespace KOA.Abilities.Behaviors
{
    /// <summary>
    /// Stun ability - prevents target from acting for a duration.
    /// </summary>
    public class StunAbility : AbilityBehavior
    {
        public override string DisplayName => "Stun";
        
        public override string[] RequiredFields => new[] { "duration" };

        public override void Execute(AbilityContext context)
        {
            int duration = context.AbilityData.duration;
            Debug.Log($"[StunAbility] Stunning target for {duration} turns");
            
            // TODO: Implement actual stun logic when status effect system is ready
            // Example: ((CardInstance)context.Target).ApplyStatusEffect(new StunEffect(duration));
            
            context.OnComplete?.Invoke();
        }
    }
}
