using UnityEngine;

namespace KOA.Abilities.Behaviors
{
    /// <summary>
    /// Draw card ability - source player draws cards.
    /// Uses 'damage' field as number of cards to draw.
    /// </summary>
    public class DrawCardAbility : AbilityBehavior
    {
        public override string DisplayName => "Draw Cards";
        
        public override string[] RequiredFields => new[] { "damage" }; // damage = cards to draw

        public override void Execute(AbilityContext context)
        {
            int cardsToDraw = context.AbilityData.damage;
            Debug.Log($"[DrawCardAbility] Drawing {cardsToDraw} cards");
            
            // TODO: Implement actual draw logic
            // Example: GameManager.Instance.GetPlayer(context.Source).DrawCards(cardsToDraw);
            
            context.OnComplete?.Invoke();
        }
    }
}
