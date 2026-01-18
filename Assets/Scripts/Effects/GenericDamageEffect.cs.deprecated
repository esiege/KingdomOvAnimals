using UnityEngine;
using KOA.Data;

namespace KOA.Effects
{
    /// <summary>
    /// Generic damage effect that reads damage value from AbilityData.
    /// Assign this to any ability that deals damage.
    /// </summary>
    public class GenericDamageEffect : AbilityEffect
    {
        public override void Execute(AbilityData data, CardController source, CardController target)
        {
            if (target == null)
            {
                Debug.LogWarning("GenericDamageEffect: No target provided.");
                return;
            }

            if (data.damage <= 0)
            {
                Debug.LogWarning($"GenericDamageEffect: Ability {data.displayName} has no damage value set.");
                return;
            }

            Debug.Log($"[Effect] {source?.cardName ?? "Unknown"} deals {data.damage} damage to {target.cardName}");
            target.TakeDamage(data.damage);
        }

        public override void Execute(AbilityData data, CardController source, PlayerController targetPlayer)
        {
            if (targetPlayer == null)
            {
                Debug.LogWarning("GenericDamageEffect: No target player provided.");
                return;
            }

            if (data.damage <= 0)
            {
                Debug.LogWarning($"GenericDamageEffect: Ability {data.displayName} has no damage value set.");
                return;
            }

            Debug.Log($"[Effect] {source?.cardName ?? "Unknown"} deals {data.damage} damage to player {targetPlayer.name}");
            targetPlayer.TakeDamage(data.damage);
        }
    }
}
