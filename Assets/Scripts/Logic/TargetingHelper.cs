using System.Collections.Generic;
using KOA.Model;
using KOA.Data;
using UnityEngine;

namespace KOA.Logic
{
    /// <summary>
    /// Helper class for calculating valid targets for abilities.
    /// Pure logic - no Unity state, making it testable.
    /// </summary>
    public static class TargetingHelper
    {
        /// <summary>
        /// Get all valid targets for a board ability (creature attacking from board).
        /// Uses the offensive ability's target type.
        /// </summary>
        public static List<(int playerId, int slotIndex)> GetValidBoardAbilityTargets(
            BoardState state, 
            int attackerPlayerId, 
            int attackerSlotIndex,
            CardLibrary cardLibrary)
        {
            var targets = new List<(int playerId, int slotIndex)>();
            
            if (state == null) return targets;
            
            var attackerCard = state.GetCard(attackerPlayerId, attackerSlotIndex);
            if (attackerCard == null || !attackerCard.IsValid) return targets;
            
            // Check if card can act
            if (!attackerCard.CanAct)
            {
                Debug.Log($"[TargetingHelper] Card at ({attackerPlayerId}, {attackerSlotIndex}) cannot act");
                return targets;
            }
            
            var cardData = cardLibrary?.GetCardById(attackerCard.CardDataId);
            if (cardData == null) return targets;
            
            // Use offensive ability's target type
            var ability = cardData.offensiveAbility;
            if (ability == null)
            {
                // Default: single enemy
                return GetTargetsForType(state, attackerPlayerId, TargetType.SingleEnemy);
            }
            
            return GetTargetsForType(state, attackerPlayerId, ability.targetType);
        }
        
        /// <summary>
        /// Get all valid targets for a flip ability (card played from hand).
        /// Uses the defensive ability's target type.
        /// </summary>
        public static List<(int playerId, int slotIndex)> GetValidFlipAbilityTargets(
            BoardState state,
            int attackerPlayerId,
            int handIndex,
            CardLibrary cardLibrary)
        {
            var targets = new List<(int playerId, int slotIndex)>();
            
            if (state == null) return targets;
            
            var player = state.GetPlayer(attackerPlayerId);
            if (handIndex < 0 || handIndex >= player.Hand.Count) return targets;
            
            var handCard = player.Hand[handIndex];
            if (handCard == null || !handCard.IsValid) return targets;
            
            var cardData = cardLibrary?.GetCardById(handCard.CardDataId);
            if (cardData == null) return targets;
            
            // Use defensive ability's target type for flip
            var ability = cardData.defensiveAbility;
            if (ability == null)
            {
                // Default: single enemy
                return GetTargetsForType(state, attackerPlayerId, TargetType.SingleEnemy);
            }
            
            return GetTargetsForType(state, attackerPlayerId, ability.targetType);
        }
        
        /// <summary>
        /// Get targets based on target type.
        /// </summary>
        private static List<(int playerId, int slotIndex)> GetTargetsForType(
            BoardState state,
            int sourcePlayerId,
            TargetType targetType)
        {
            var targets = new List<(int playerId, int slotIndex)>();
            int enemyPlayerId = 1 - sourcePlayerId;
            
            var player = state.GetPlayer(sourcePlayerId);
            var enemy = state.GetOpponent(sourcePlayerId);
            
            switch (targetType)
            {
                case TargetType.Self:
                    // Find source card slot - this would need additional context
                    // For now, don't add any targets
                    break;
                    
                case TargetType.SingleEnemy:
                    for (int i = 0; i < 3; i++)
                    {
                        if (!enemy.IsSlotEmpty(i))
                        {
                            targets.Add((enemyPlayerId, i));
                        }
                    }
                    break;
                    
                case TargetType.SingleAlly:
                    for (int i = 0; i < 3; i++)
                    {
                        if (!player.IsSlotEmpty(i))
                        {
                            targets.Add((sourcePlayerId, i));
                        }
                    }
                    break;
                    
                case TargetType.AllEnemies:
                    // For area effects, return all enemies
                    for (int i = 0; i < 3; i++)
                    {
                        if (!enemy.IsSlotEmpty(i))
                        {
                            targets.Add((enemyPlayerId, i));
                        }
                    }
                    break;
                    
                case TargetType.AllAllies:
                    for (int i = 0; i < 3; i++)
                    {
                        if (!player.IsSlotEmpty(i))
                        {
                            targets.Add((sourcePlayerId, i));
                        }
                    }
                    break;
                    
                case TargetType.BoardWide:
                    // All cards on board
                    for (int i = 0; i < 3; i++)
                    {
                        if (!player.IsSlotEmpty(i))
                            targets.Add((sourcePlayerId, i));
                        if (!enemy.IsSlotEmpty(i))
                            targets.Add((enemyPlayerId, i));
                    }
                    break;
                    
                case TargetType.EnemyPlayer:
                    // Player targeting - would need different handling
                    // For now, return empty (player attack handled separately)
                    break;
            }
            
            return targets;
        }
        
        /// <summary>
        /// Get valid empty slots for playing a card.
        /// </summary>
        public static List<int> GetValidPlaySlots(BoardState state, int playerId)
        {
            var slots = new List<int>();
            
            if (state == null) return slots;
            
            var player = state.GetPlayer(playerId);
            
            for (int i = 0; i < 3; i++)
            {
                if (player.IsSlotEmpty(i))
                {
                    slots.Add(i);
                }
            }
            
            return slots;
        }
        
        /// <summary>
        /// Check if a specific target is valid.
        /// </summary>
        public static bool IsValidTarget(
            List<(int playerId, int slotIndex)> validTargets,
            int targetPlayerId,
            int targetSlotIndex)
        {
            return validTargets.Contains((targetPlayerId, targetSlotIndex));
        }
        
        /// <summary>
        /// Get max health for a card from CardData.
        /// </summary>
        public static int GetMaxHealth(CardState card, CardLibrary library)
        {
            var data = library?.GetCardById(card.CardDataId);
            return data?.health ?? card.CurrentHealth;
        }
    }
}
