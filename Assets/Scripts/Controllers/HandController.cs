using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    // List of cards in the hand
    private List<CardController> hand;

    // Card positions in hand
    public List<GameObject> cardPositions;

    // Currently focused card
    private HoverHandler focusedCardHandler;

    // Hover effect offset and transition speed
    public Vector3 hoverOffset = new Vector3(0, 1f, 0);
    public float transitionSpeed = 5f;

    void Awake()
    {
        hand = new List<CardController>();
    }

    // Add a card to the hand at a specific position
    public void AddCardToHand(CardController card)
    {
        if (hand.Count < cardPositions.Count)
        {
            int positionIndex = hand.Count;
            hand.Add(card);

            // Instantiate and position the card in the hand
            GameObject cardObject = Instantiate(card.gameObject, cardPositions[positionIndex].transform.position, Quaternion.identity);
            cardObject.transform.SetParent(cardPositions[positionIndex].transform);

            // Add the HoverHandler to manage focus changes
            HoverHandler hoverHandler = cardObject.AddComponent<HoverHandler>();
            hoverHandler.Initialize(this, card, cardPositions[positionIndex].transform.localPosition, hoverOffset, transitionSpeed);
        }
        else
        {
            Debug.LogError("Hand is full. Cannot add more cards.");
        }
    }

    // Remove a card from the hand
    public void RemoveCardFromHand(CardController card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            Destroy(card.gameObject);
            // Optional: You can add logic to rearrange the remaining cards
        }
        else
        {
            Debug.LogWarning("Attempted to remove a card that is not in the hand.");
        }
    }

    // Get the list of cards in the hand
    public List<CardController> GetHand()
    {
        return hand;
    }

    // Set a card as the focused card
    public void SetFocusedCard(HoverHandler newFocusedCardHandler)
    {
        // Unfocus the previously focused card
        if (focusedCardHandler != null && focusedCardHandler != newFocusedCardHandler)
        {
            focusedCardHandler.ReturnToOriginalPosition();
        }

        // Set the new focused card
        focusedCardHandler = newFocusedCardHandler;
    }
}
