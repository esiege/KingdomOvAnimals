using UnityEngine;
using KOA.Data;

namespace KOA.Effects
{
    /// <summary>
    /// Base class for all ability effects.
    /// Effects contain the BEHAVIOR (code) that reads from AbilityData (values).
    /// </summary>
    public abstract class AbilityEffect : MonoBehaviour
    {
        /// <summary>
        /// Execute this effect using data from AbilityData.
        /// </summary>
        /// <param name="data">The ability data containing values like damage, heal, etc.</param>
        /// <param name="source">The card that is using this ability.</param>
        /// <param name="target">The card being targeted (can be null for self-targeting).</param>
        public abstract void Execute(AbilityData data, CardController source, CardController target);

        /// <summary>
        /// Execute this effect targeting a player instead of a card.
        /// </summary>
        /// <param name="data">The ability data containing values.</param>
        /// <param name="source">The card that is using this ability.</param>
        /// <param name="targetPlayer">The player being targeted.</param>
        public virtual void Execute(AbilityData data, CardController source, PlayerController targetPlayer)
        {
            // Default implementation - override in subclasses that support player targeting
            Debug.LogWarning($"Effect {GetType().Name} does not support player targeting.");
        }

        /// <summary>
        /// Optional: Called before the effect executes for setup/validation.
        /// </summary>
        public virtual bool CanExecute(AbilityData data, CardController source, CardController target)
        {
            return true;
        }
    }
}
