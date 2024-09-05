using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    private List<CardController> hand = new List<CardController>();
    private List<HoverHandler> hoveredCards = new List<HoverHandler>();
    private HoverHandler focusedCardHandler;
    private CardController activeCard;
    private Vector3 activeCardPosition;
    private LineRenderer lineRenderer;

    public List<GameObject> cardPositions;
    public EncounterController encounterController;
    public float transitionSpeed = 5f;
    public float zIncrement = 0.1f;
    public Material lineMaterial;

    private Vector3 hoverOffset = new Vector3(0, 1.8f, 0);

    void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.05f;
        lineRenderer.enabled = false;
        lineRenderer.material = lineMaterial;
    }

    void Start() => StartCoroutine(CheckFocusedCard());

    void Update() => HandleMouseInput();

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            HandleMouseDown();
        if (Input.GetMouseButton(0))
            OnMouseDrag();
        if (Input.GetMouseButtonUp(0))
            OnMouseUp();
    }

    private void HandleMouseDown()
    {
        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
        {
            var clickedCard = hit.collider.GetComponent<CardController>();
            if (clickedCard != null && hand.Contains(clickedCard))
                OnCardMouseDown(clickedCard);
            else
                Debug.Log("Card cannot be made active because it is not in the player's hand.");
        }
    }

    public void AddCardToHand(CardController card)
    {
        if (hand.Count >= cardPositions.Count)
        {
            Debug.LogError("Hand is full. Cannot add more cards.");
            return;
        }

        int positionIndex = hand.Count;
        GameObject cardObject = Instantiate(card.gameObject, cardPositions[positionIndex].transform.position, Quaternion.identity);
        cardObject.transform.SetParent(cardPositions[positionIndex].transform);

        CardController instantiatedCard = cardObject.GetComponent<CardController>();
        hand.Add(instantiatedCard);

        if (cardObject.GetComponent<BoxCollider2D>() == null)
            cardObject.AddComponent<BoxCollider2D>();

        HoverHandler hoverHandler = cardObject.AddComponent<HoverHandler>();
        hoverHandler.Initialize(this, instantiatedCard, positionIndex, cardPositions[positionIndex].transform.localPosition, hoverOffset, transitionSpeed);

        ArrangeCardsInHand();
    }

    public void RemoveCardFromHand(CardController card)
    {
        if (hand.Remove(card))
        {
            Destroy(card.gameObject);
            ArrangeCardsInHand();
        }
        else
            Debug.LogWarning("Attempted to remove a card that is not in the hand.");
    }

    private void OnCardMouseDown(CardController card)
    {
        activeCard = card;
        activeCardPosition = card.transform.position;
        card.GetComponent<HoverHandler>().enabled = false;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, activeCardPosition);
        activeCard.isActive = true;
    }

    private void OnMouseDrag()
    {
        if (activeCard == null) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        lineRenderer.SetPosition(1, mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider == null)
        {
            SetLineColor(activeCard.isInPlay ? new Color(0.3f, 0.3f, 0.3f) : Color.white);
            return;
        }

        GameObject hitObject = hit.collider.gameObject;
        if (hitObject.name.StartsWith("FreeSlot"))
            show_addCardToEncounter(activeCard);
        else if (hitObject.TryGetComponent(out CardController targetCard))
        {
            if (targetCard.owningPlayer == encounterController.currentPlayer)
                show_useCardAbilityDefensive(targetCard);
            else
                show_useCardAbilityOffensive(targetCard);
        }
    }

    private void show_addCardToEncounter(CardController card)
    {
        SetLineColor(activeCard.isInPlay ? new Color(0.3f, 0.3f, 0.3f) : Color.yellow);
        Debug.Log($"Card {card.cardName} is being added to the encounter.");
    }

    private void addCardToEncounter(CardController card, GameObject hitObject)
    {
        if (activeCard.isInPlay) return;

        card.transform.SetParent(hitObject.transform);
        card.transform.position = hitObject.transform.position;
        card.transform.localPosition = Vector3.zero;

        hitObject.name = hitObject.name.Replace("FreeSlot", "FriendlyUnit");
        Debug.Log($"Slot renamed to {hitObject.name}");

        card.isInPlay = true;
        card.isActive = false;
        card.isInHand = false;
    }

    private void show_useCardAbilityDefensive(CardController targetCard)
    {
        SetLineColor(activeCard.isInPlay ? Color.cyan : Color.green);
        Debug.Log($"Using defensive ability of {activeCard.cardName} on {targetCard.cardName}.");
    }

    private void useCardAbilityDefensive(CardController targetCard)
    {
        Debug.Log("Defense action triggered!");
    }

    private void show_useCardAbilityOffensive(CardController targetCard)
    {
        SetLineColor(activeCard.isInPlay ? new Color(1.0f, 0.5f, 0.0f) : Color.red);
        Debug.Log($"Using offensive ability of {activeCard.cardName} on {targetCard.cardName}.");
    }

    private void useCardAbilityOffensive(CardController targetCard)
    {
        Debug.Log("Offense action triggered!");
    }

    private void SetLineColor(Color color)
    {
        lineRenderer.startColor = lineRenderer.endColor = color;
    }

    private void OnMouseUp()
    {
        if (activeCard == null) return;

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.name.StartsWith("FreeSlot"))
                addCardToEncounter(activeCard, hitObject);
            else if (hitObject.TryGetComponent(out CardController targetCard))
            {
                if (targetCard.owningPlayer == encounterController.currentPlayer)
                    useCardAbilityDefensive(targetCard);
                else
                    useCardAbilityOffensive(targetCard);

                activeCard.isInPlay = false;
                activeCard.isActive = false;
                activeCard.isInHand = true;
            }
        }

        activeCard.GetComponent<HoverHandler>().enabled = true;
        activeCard = null;
        lineRenderer.enabled = false;
    }

    public List<CardController> GetHand() => hand;

    public void AddHoveredCard(HoverHandler hoverHandler)
    {
        if (!hoveredCards.Contains(hoverHandler))
            hoveredCards.Add(hoverHandler);
    }

    public void RemoveHoveredCard(HoverHandler hoverHandler)
    {
        hoveredCards.Remove(hoverHandler);
    }

    private IEnumerator CheckFocusedCard()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            HoverHandler cardToFocus = null;
            int highestIndex = -1;

            foreach (var hoverHandler in hoveredCards)
            {
                if (hoverHandler.CardIndex > highestIndex)
                {
                    highestIndex = hoverHandler.CardIndex;
                    cardToFocus = hoverHandler;
                }
            }

            if (cardToFocus != focusedCardHandler)
            {
                focusedCardHandler?.ReturnToOriginalPosition();
                focusedCardHandler = cardToFocus;
                focusedCardHandler?.StartHovering();
            }
        }
    }

    private void ArrangeCardsInHand()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            GameObject cardObject = hand[i].gameObject;
            Vector3 cardPosition = cardObject.transform.localPosition;
            cardPosition.z = zIncrement * i;
            cardObject.transform.localPosition = cardPosition;
        }
    }
}