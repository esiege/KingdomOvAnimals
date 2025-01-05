using UnityEngine;

public enum AbilityTargetType
{
    SingleEnemy,     // Target one enemy card
    SingleFriendly,  // Target one friendly card
    AllEnemies,      // Target all enemy cards
    AllFriendlies,   // Target all friendly cards
    Self,            // Target self
    PlayerOnly,      // Target the opponent player directly
    BoardWide        // Affect all cards on the board
}

public abstract class AbilityController : MonoBehaviour
{

    public AbilityTargetType targetType; // Determines valid targets

    // Activate ability on a CardController target
    public abstract void Activate(CardController target);

    // Overloaded: Activate ability on a PlayerController target
    public abstract void Activate(PlayerController target);
}
