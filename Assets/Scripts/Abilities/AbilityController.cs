using UnityEngine;

public abstract class AbilityController : MonoBehaviour
{
    // Method to be implemented by derived classes
    public abstract void Activate(CardController target);
}
