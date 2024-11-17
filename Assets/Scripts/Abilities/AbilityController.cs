using UnityEngine;

public abstract class AbilityController : MonoBehaviour
{
    // Activate ability on a CardController target
    public abstract void Activate(CardController target);

    // Overloaded: Activate ability on a PlayerController target
    public abstract void Activate(PlayerController target);
}
