using System.Collections.Generic;
using UnityEngine;

public class TargetingController : MonoBehaviour
{
    public EncounterController encounterController; // Reference to the EncounterController

    // Get a list of playable cards in the player's hand
    public List<CardController> GetPlayableCardsInHand()
    {
        List<CardController> playableCards = new List<CardController>();

        HandController handController = encounterController.currentPlayer == encounterController.player
            ? encounterController.playerHandController
            : encounterController.opponentHandController;

        if (!HasOpenBoardSlot(encounterController.currentPlayer))
        {
            Debug.Log("No open slots available.");
            return playableCards;
        }

        foreach (CardController card in handController.GetHand())
        {
            if (encounterController.currentPlayer.currentMana >= card.manaCost)
            {
                playableCards.Add(card);
            }
        }

        return playableCards;
    }

    // Get a list of usable cards on the board
    public List<CardController> GetUsableCardsOnBoard()
    {
        List<CardController> usableCards = new List<CardController>();

        foreach (CardController card in encounterController.currentPlayer.board)
        {
            if (card.owningPlayer == encounterController.currentPlayer &&
                card.isInPlay &&
                !card.hasSummoningSickness &&
                !card.isTapped)
            {
                if (GetOffensiveTargets(card).Count > 0 || GetSupportTargets(card).Count > 0)
                {
                    usableCards.Add(card);
                }
            }
        }

        return usableCards;
    }

    // Get a list of offensive targets based on the ability's target type
    public List<GameObject> GetOffensiveTargets(CardController card)
    {
        List<GameObject> offensiveTargets = new List<GameObject>();

        AbilityController offensiveAbility = card.offensiveAbility?.GetComponentInChildren<AbilityController>();
        if (offensiveAbility == null)
        {
            Debug.LogError($"No offensive ability found for card {card.cardName}");
            return offensiveTargets;
        }

        AbilityTargetType targetType = offensiveAbility.targetType;

        switch (targetType)
        {
            case AbilityTargetType.SingleEnemy:
                offensiveTargets.Add(GetFirstEnemyTarget() ?? encounterController.opponent.gameObject);
                break;

            case AbilityTargetType.AllEnemies:
                AddAllEnemyTargets(offensiveTargets);
                break;

            case AbilityTargetType.BoardWide:
                AddAllBoardTargets(offensiveTargets);
                break;

            default:
                Debug.LogWarning($"Unsupported offensive target type: {targetType}");
                break;
        }

        offensiveTargets.RemoveAll(target => target == null);
        return offensiveTargets;
    }

    // Get a list of support targets based on the ability's target type
    public List<GameObject> GetSupportTargets(CardController card)
    {
        List<GameObject> supportTargets = new List<GameObject>();

        AbilityController supportAbility = card.supportAbility?.GetComponentInChildren<AbilityController>();
        if (supportAbility == null)
        {
            Debug.LogError($"No support ability found for card {card.cardName}");
            return supportTargets;
        }

        AbilityTargetType targetType = supportAbility.targetType;

        switch (targetType)
        {
            case AbilityTargetType.SingleFriendly:
                AddAllFriendlyTargets(supportTargets);  // Add all friendly targets so the player can choose any
                supportTargets.Add(encounterController.currentPlayer.gameObject); // Add the player as a possible target
                break;

            case AbilityTargetType.AllFriendlies:
                AddAllFriendlyTargets(supportTargets);
                break;

            case AbilityTargetType.PlayerOnly:
                supportTargets.Add(encounterController.currentPlayer.gameObject);
                break;

            case AbilityTargetType.BoardWide:
                AddAllBoardTargets(supportTargets);
                break;

            default:
                Debug.LogWarning($"Unsupported support target type: {targetType}");
                break;
        }

        supportTargets.RemoveAll(target => target == null);
        return supportTargets;
    }

    // Helper: Add all enemy cards from the opponent's slots
    private void AddAllEnemyTargets(List<GameObject> targets)
    {
        targets.Add(GetCardInSlot("OpponentSlot-1")?.gameObject);
        targets.Add(GetCardInSlot("OpponentSlot-2")?.gameObject);
        targets.Add(GetCardInSlot("OpponentSlot-3")?.gameObject);
    }

    // Helper: Add all friendly cards from the player's slots
    private void AddAllFriendlyTargets(List<GameObject> targets)
    {
        targets.Add(GetCardInSlot("PlayerSlot-1")?.gameObject);
        targets.Add(GetCardInSlot("PlayerSlot-2")?.gameObject);
        targets.Add(GetCardInSlot("PlayerSlot-3")?.gameObject);
    }

    // Helper: Add all board-wide targets (all slots on both sides)
    private void AddAllBoardTargets(List<GameObject> targets)
    {
        AddAllEnemyTargets(targets);
        AddAllFriendlyTargets(targets);
    }

    // Helper: Get the first enemy card in a slot
    private GameObject GetFirstEnemyTarget()
    {
        return GetCardInSlot("OpponentSlot-1")?.gameObject ??
               GetCardInSlot("OpponentSlot-2")?.gameObject ??
               GetCardInSlot("OpponentSlot-3")?.gameObject;
    }

    // Helper: Check for open slots on the player's side of the board
    private bool HasOpenBoardSlot(PlayerController player)
    {
        GameObject slot1 = GameObject.Find("PlayerSlot-1");
        GameObject slot2 = GameObject.Find("PlayerSlot-2");
        GameObject slot3 = GameObject.Find("PlayerSlot-3");
        
        // Return false if slots aren't found (scene still loading)
        if (slot1 == null || slot2 == null || slot3 == null)
        {
            Debug.LogWarning("[TargetingController] Board slots not found - scene may still be loading");
            return false;
        }
        
        return slot1.GetComponentInChildren<CardController>() == null ||
               slot2.GetComponentInChildren<CardController>() == null ||
               slot3.GetComponentInChildren<CardController>() == null;
    }

    // Helper: Get the card occupying a specific slot
    private CardController GetCardInSlot(string slotName)
    {
        GameObject slot = GameObject.Find(slotName);
        return slot?.GetComponentInChildren<CardController>();
    }
}
