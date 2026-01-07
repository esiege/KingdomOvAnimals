using UnityEngine;

namespace KOA.Abilities.Behaviors
{
    /// <summary>
    /// Return to hand ability - returns the target card to owner's hand.
    /// </summary>
    public class ReturnToHandAbility : AbilityBehavior
    {
        public override string DisplayName => "Return to Hand";
        
        public override string[] RequiredFields => new string[0]; // No additional fields needed

        public override void Execute(AbilityContext context)
        {
            Debug.Log($"[ReturnToHandAbility] Returning target to owner's hand");
            
            // TODO: Implement actual return to hand logic
            // Example: GameManager.Instance.ReturnCardToHand((CardInstance)context.Target);
            
            context.OnComplete?.Invoke();
        }
    }
}
