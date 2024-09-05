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

    public EncounterController encounterController;

    // Hover effect offset and transition speed
    private Vector3 hoverOffset = new Vector3(0, 1.8f, 0);
    public float transitionSpeed = 5f;

    // Z-axis increment for rendering cards above one another
    public float zIncrement = 0.1f;

    // Variables for active card interaction
    private CardController activeCard;
    private Vector3 activeCardPosition;
    private LineRenderer lineRenderer;
    public Material lineMaterial;

    void Awake()
    {
        hand = new List<CardController>();
        hoveredCards = new List<HoverHandler>();

        // Initialize LineRenderer for drawing the line between card and mouse
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.enabled = false;
        lineRenderer.material = lineMaterial;
    }

    void Start()
    {
        // Start the periodic focus check
        StartCoroutine(CheckFocusedCard());
    }

    void Update()
    {
        HandleMouseInput();
    }

    // Handle mouse input manually using Input.GetMouseButton
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                // Get the CardController component from the clicked card
                CardController clickedCard = hit.collider.GetComponent<CardController>();

                // Check if the clicked card is in the player's hand before making it active
                if (clickedCard != null && hand.Contains(clickedCard))
                {
                    OnCardMouseDown(clickedCard);
                }
                else
                {
                    Debug.Log("Card cannot be made active because it is not in the player's hand.");
                }
            }
        }

        if (Input.GetMouseButton(0)) // Mouse is held down
        {
            OnMouseDrag();
        }

        if (Input.GetMouseButtonUp(0)) // Mouse button released
        {
            OnMouseUp();
        }
    }

    // Add a card to the hand at a specific position
    public void AddCardToHand(CardController card)
    {
        if (hand.Count < cardPositions.Count)
        {
            int positionIndex = hand.Count;

            // Instantiate the card and position it in the hand
            GameObject cardObject = Instantiate(card.gameObject, cardPositions[positionIndex].transform.position, Quaternion.identity);
            cardObject.transform.SetParent(cardPositions[positionIndex].transform);

            // Retrieve the CardController of the instantiated object
            CardController instantiatedCard = cardObject.GetComponent<CardController>();

            // Add the instantiated card to the hand list
            hand.Add(instantiatedCard);

            // Add a BoxCollider2D if not present (for click detection)
            if (cardObject.GetComponent<BoxCollider2D>() == null)
            {
                cardObject.AddComponent<BoxCollider2D>();
            }

            // Add the HoverHandler to manage focus changes
            HoverHandler hoverHandler = cardObject.AddComponent<HoverHandler>();
            hoverHandler.Initialize(this, instantiatedCard, positionIndex, cardPositions[positionIndex].transform.localPosition, hoverOffset, transitionSpeed);

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

    // Handle mouse down event for activating a card

    private void OnCardMouseDown(CardController card)
    {
        // Activate the card and freeze its position at the current point
        activeCard = card;
        activeCardPosition = card.transform.position;  // Freeze card in place
        card.GetComponent<HoverHandler>().enabled = false;  // Disable hover effect when active
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, activeCardPosition);

        // Update the card's status
        activeCard.isActive = true;  // The card is now in an active state
    }


    private void OnMouseDrag()
    {
        if (activeCard == null) return;

        // Update the line to follow the mouse position
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        lineRenderer.SetPosition(1, mousePosition);

        // Raycast to detect the object under the mouse
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider == null)
        {
            SetLineColor(activeCard.isInPlay ? new Color(0.3f, 0.3f, 0.3f) : Color.white); // Dark gray if in play, white otherwise
            return;
        }

        GameObject hitObject = hit.collider.gameObject;
        if (hitObject.name.StartsWith("FreeSlot"))
        {
            show_addCardToEncounter(activeCard);  // Dragging over a free slot
        }
        else if (hitObject.TryGetComponent(out CardController targetCard))
        {
            if (targetCard.owningPlayer == encounterController.currentPlayer)
            {
                show_useCardAbilityDefensive(targetCard);  // Dragging over a friendly (owned) card
            }
            else
            {
                show_useCardAbilityOffensive(targetCard);  // Dragging over an enemy card
            }
        }
    }

    // Called when the card is dragged over a free slot
    private void show_addCardToEncounter(CardController card)
    {
        SetLineColor(activeCard.isInPlay ? new Color(0.3f, 0.3f, 0.3f) : Color.yellow);  // Dark gray if in play, yellow otherwise
        Debug.Log($"Card {card.cardName} is being added to the encounter.");
        // Implement your logic here for adding the card to the encounter
    }
    private void addCardToEncounter(CardController card, GameObject hitObject)
    {
        if (activeCard.isInPlay)
            return;

        Debug.Log("Card played on free slot!");

        // Set the card's parent to the hitObject (free slot)
        card.transform.SetParent(hitObject.transform);

        // Center the card on the free slot's position
        card.transform.position = hitObject.transform.position;

        // Optionally reset local position if needed (e.g., if you have offsets)
        card.transform.localPosition = Vector3.zero;

        // Rename the slot from FreeSlot-X to FriendlyUnit-X
        string newName = hitObject.name.Replace("FreeSlot", "FriendlyUnit");
        hitObject.name = newName;

        Debug.Log($"Slot renamed to {newName}");

        // Update card's status: it's now in play
        card.isInPlay = true;
        card.isActive = false;
        card.isInHand = false;
    }





    // Called when the card is dragged over a friendly (owned) card
    private void show_useCardAbilityDefensive(CardController targetCard)
    {
        SetLineColor(activeCard.isInPlay ? Color.cyan : Color.green);  // Teal if in play, green otherwise
        Debug.Log($"Using defensive ability of {activeCard.cardName} on {targetCard.cardName}.");
        // Implement the logic here for using the defensive ability
    }
    private void useCardAbilityDefensive(CardController targetCard)
    {
        Debug.Log("Defense action triggered!");

        // Placeholder for actual defensive action logic
        // Example: targetCard.BoostDefense(); (if you have such a method)
    }


    // Called when the card is dragged over an enemy card
    private void show_useCardAbilityOffensive(CardController targetCard)
    {
        SetLineColor(activeCard.isInPlay ? new Color(1.0f, 0.5f, 0.0f) : Color.red);  // Orange if in play, red otherwise
        Debug.Log($"Using offensive ability of {activeCard.cardName} on {targetCard.cardName}.");
        // Implement the logic here for using the offensive ability
    }
    private void useCardAbilityOffensive(CardController targetCard)
    {
        Debug.Log("Offense action triggered!");

        // Placeholder for actual offensive action logic
        // Example: activeCard.Attack(targetCard); (if you have such a method)
    }


    // Helper method to set both start and end color of the line renderer
    private void SetLineColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }





    private void OnMouseUp()
    {
        if (activeCard == null) return;

        // Raycast to detect the release target
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;

            // Check if the hit object is a FreeSlot
            if (hitObject.name.StartsWith("FreeSlot"))
            {
                addCardToEncounter(activeCard, hitObject);
            }
            else if (hitObject.GetComponent<CardController>() != null) // If the hit object is another card
            {
                CardController targetCard = hitObject.GetComponent<CardController>();

                if (targetCard.owningPlayer == encounterController.currentPlayer) // Friendly card
                {
                    useCardAbilityDefensive(targetCard);
                }
                else // Enemy card
                {
                    useCardAbilityOffensive(targetCard);
                }

                // Update card's status: it's still in hand, but not active anymore
                activeCard.isInPlay = false;
                activeCard.isActive = false;
                activeCard.isInHand = true;
            }
        }

        // Deactivate the card and remove the line
        activeCard.GetComponent<HoverHandler>().enabled = true;
        activeCard = null;
        lineRenderer.enabled = false;
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
