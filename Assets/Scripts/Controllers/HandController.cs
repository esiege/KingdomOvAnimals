using System.Collections;
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

    // Hovered cards list
    private List<HoverHandler> hoveredCards;

    // Hover effect offset and transition speed
    private Vector3 hoverOffset = new Vector3(0, 1.8f, 0);
    public float transitionSpeed = 5f;

    // Z-axis increment for rendering cards above one another
    public float zIncrement = 0.1f;

    void Awake()
    {
        hand = new List<CardController>();
        hoveredCards = new List<HoverHandler>();
    }

    void Start()
    {
        // Start the periodic focus check
        StartCoroutine(CheckFocusedCard());
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
            hoverHandler.Initialize(this, card, positionIndex, cardPositions[positionIndex].transform.localPosition, hoverOffset, transitionSpeed);

            // Arrange the cards in hand to adjust Z positions
            ArrangeCardsInHand();
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

            // Arrange the remaining cards in hand to adjust Z positions
            ArrangeCardsInHand();
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

    // Add a card to the hovered list
    public void AddHoveredCard(HoverHandler hoverHandler)
    {
        if (!hoveredCards.Contains(hoverHandler))
        {
            hoveredCards.Add(hoverHandler);
        }
    }

    // Remove a card from the hovered list
    public void RemoveHoveredCard(HoverHandler hoverHandler)
    {
        if (hoveredCards.Contains(hoverHandler))
        {
            hoveredCards.Remove(hoverHandler);
        }
    }

    // Periodically check which card should be focused
    private IEnumerator CheckFocusedCard()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            HoverHandler cardToFocus = null;
            int highestIndex = -1;

            // Find the card with the highest index
            foreach (var hoverHandler in hoveredCards)
            {
                if (hoverHandler.CardIndex > highestIndex)
                {
                    highestIndex = hoverHandler.CardIndex;
                    cardToFocus = hoverHandler;
                }
            }

            // Update the focused card
            if (cardToFocus != focusedCardHandler)
            {
                // Unfocus the previously focused card
                if (focusedCardHandler != null)
                {
                    focusedCardHandler.ReturnToOriginalPosition();
                }

                // Set the new focused card
                focusedCardHandler = cardToFocus;

                if (focusedCardHandler != null)
                {
                    focusedCardHandler.StartHovering();
                }
            }
        }
    }

    // Arrange the Z position of cards in hand based on their index
    private void ArrangeCardsInHand()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            CardController card = hand[i];
            GameObject cardObject = card.gameObject;

            // Calculate the Z position for each card
            float zPosition = zIncrement * i; // Increment Z value based on index

            // Set the Z position of the card's parent transform (or card itself)
            Vector3 cardPosition = cardObject.transform.localPosition;
            cardPosition.z = zPosition; // Apply calculated Z position
            cardObject.transform.localPosition = cardPosition;

            Debug.Log($"Card {i} placed at Z position: {zPosition}");
        }
    }
}
