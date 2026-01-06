using UnityEngine;
using KOA.Data;

namespace KOA.Effects
{
    /// <summary>
    /// Generic heal effect that reads heal value from AbilityData.
    /// Assign this to any ability that heals.
    /// </summary>
    public class GenericHealEffect : AbilityEffect
    {
        public override void Execute(AbilityData data, CardController source, CardController target)
        {
            if (target == null)
            {
                Debug.LogWarning("GenericHealEffect: No target provided.");
                return;
            }

            if (data.healAmount <= 0)
            {
                Debug.LogWarning($"GenericHealEffect: Ability {data.displayName} has no heal amount set.");
                return;
            }

            Debug.Log($"[Effect] {source?.cardName ?? "Unknown"} heals {target.cardName} for {data.healAmount}");
            target.Heal(data.healAmount);
        }

        public override void Execute(AbilityData data, CardController source, PlayerController targetPlayer)
        {
            if (targetPlayer == null)
            {
                Debug.LogWarning("GenericHealEffect: No target player provided.");
                return;
            }

            if (data.healAmount <= 0)
            {
                Debug.LogWarning($"GenericHealEffect: Ability {data.displayName} has no heal amount set.");
                return;
            }

            Debug.Log($"[Effect] {source?.cardName ?? "Unknown"} heals player {targetPlayer.name} for {data.healAmount}");
            targetPlayer.Heal(data.healAmount);
        }
    }
}
