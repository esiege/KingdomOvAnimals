using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class HandController : MonoBehaviour
{
    private List<CardController> playerHand = new List<CardController>();
    private List<HoverHandler> hoveredCards = new List<HoverHandler>();
    private HoverHandler focusedCardHandler;
    private CardController activeCard;
    private Vector3 activeCardPosition;
    private LineRenderer lineRenderer;

    public List<GameObject> cardPositions;
    public EncounterController encounterController;
    public float transitionSpeed = 5f;
    public float zIncrement = 0.2f;
    public Material lineMaterial;

    // Owner reference
    public PlayerController owningPlayer;

    private Vector3 hoverOffset = new Vector3(0, 1.8f, 0);

    void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.05f;
        lineRenderer.enabled = false;
        lineRenderer.material = lineMaterial;
    }



    void Start()
    {
        StartCoroutine(CheckFocusedCard());
        VisualizePlayableHand();
        HideBoardTargets();
    }


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
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Ensure the position stays in the 2D plane

        // Get all colliders at the mouse position
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePosition);

        // Check each collider for a CardController
        foreach (var hit in hits)
        {
            var clickedCard = hit.GetComponent<CardController>();
            if (clickedCard != null && clickedCard.owningPlayer == encounterController.currentPlayer)
            {
                OnCardMouseDown(clickedCard);
                return; // Stop once the correct CardController is found
            }
        }
    }


    public void AddCardToHand(CardController card)
    {
        if (playerHand.Count >= cardPositions.Count) //?
        {
            Debug.LogError("Hand is full. Cannot add more cards.");
            return;
        }

        int positionIndex = playerHand.Count;

        // Instantiate the card and position it correctly in the hand
        GameObject cardObject = Instantiate(card.gameObject, cardPositions[positionIndex].transform.position, Quaternion.identity);
        cardObject.transform.SetParent(cardPositions[positionIndex].transform);

        CardController instantiatedCard = cardObject.GetComponent<CardController>();
        playerHand.Add(instantiatedCard);

        if (cardObject.GetComponent<BoxCollider2D>() == null)
            cardObject.AddComponent<BoxCollider2D>();

        //HoverHandler hoverHandler = cardObject.AddComponent<HoverHandler>();
        //hoverHandler.Initialize(this, instantiatedCard, positionIndex, cardPositions[positionIndex].transform.localPosition, hoverOffset, transitionSpeed);

        // Re-arrange the cards in hand to ensure proper positioning
        ArrangeCardsInHand();
    }

    public void RemoveCardFromHand(CardController card)
    {
        int removedIndex = playerHand.IndexOf(card);  // Get the index of the card being removed
        if (removedIndex >= 0)
        {
            // Remove the card from the hand
            playerHand.RemoveAt(removedIndex);

            // Re-arrange the cards to ensure the gaps are filled
            ArrangeCardsInHand();
        }
        else
        {
            Debug.LogWarning("Attempted to remove a card that is not in the hand.");
        }
    }

    // Method to arrange the cards in hand and update their positions
    private void ArrangeCardsInHand()
    {
        for (int i = 0; i < playerHand.Count; i++)
        {
            GameObject cardObject = playerHand[i].gameObject;

            // Move the card to its new position based on the current index
            cardObject.transform.SetParent(cardPositions[i].transform);
            cardObject.transform.localPosition = Vector3.zero;  // Reset the local position within the slot

            // Adjust Z position so the cards appear stacked correctly
            Vector3 cardPosition = cardObject.transform.localPosition;
            cardPosition.z = zIncrement * i;
            cardObject.transform.localPosition = cardPosition;

            // Set the sorting order for layering
            SpriteRenderer spriteRenderer = cardObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = playerHand.Count - i; // Higher sortingOrder for leftmost card
            }

            // Update any other logic associated with the card (like hover behavior)
            HoverHandler hoverHandler = cardObject.GetComponent<HoverHandler>();
            if (hoverHandler != null)
            {
                hoverHandler.CardIndex = i;  // Ensure the hover handler has the correct index
            }
        }
    }


    public List<GameObject> GetAllBoardTargets()
    {
        List<GameObject> targets = new List<GameObject>();

        targets.Add(GameObject.Find("OpponentSlot-1"));
        targets.Add(GameObject.Find("OpponentSlot-2"));
        targets.Add(GameObject.Find("OpponentSlot-3"));
        targets.Add(GameObject.Find("PlayerSlot-1"));
        targets.Add(GameObject.Find("PlayerSlot-2"));
        targets.Add(GameObject.Find("PlayerSlot-3"));

        //targets.Add(GameObject.Find("Player"));
        //targets.Add(GameObject.Find("Opponent"));

        return targets;
    }
    public List<GameObject> GetPlayerPlayableBoard()
    {
        List<GameObject> targets = new List<GameObject>();

        targets.Add(GameObject.Find("PlayerSlot-1"));
        targets.Add(GameObject.Find("PlayerSlot-2"));
        targets.Add(GameObject.Find("PlayerSlot-3"));

        //targets.Add(GameObject.Find("Player"));
        //targets.Add(GameObject.Find("Opponent"));

        return targets;
    }
    public bool AllPlayerSlotsFull()
    {
        int cnt = 0;

        if (GameObject.Find("PlayerSlot-1").GetComponentInChildren<CardController>() != null)
            cnt++;
        if (GameObject.Find("PlayerSlot-2").GetComponentInChildren<CardController>() != null)
            cnt++;
        if (GameObject.Find("PlayerSlot-3").GetComponentInChildren<CardController>() != null)
            cnt++;

        return cnt == 3;
    }

    private void OnCardMouseDown(CardController card)
    {
        activeCard = card;
        activeCardPosition = card.transform.position;
        //card.GetComponent<HoverHandler>().enabled = false;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, activeCardPosition);
        activeCard.isActive = true;
        HidePlayableHand();
        VisualizeBoardTargets();

    }


    public void VisualizeBoardTargets()
    {

        List<GameObject> offensiveTargets = activeCard.offensiveAbility.GetComponentInChildren<DamageAbility>().GetHighlightTargets();
        foreach (var t in offensiveTargets)
        {
            CardController c = t.GetComponentInChildren<CardController>();

            if (c == null || owningPlayer.name != "Player") continue;

            if (activeCard.isInHand)
            {
                if (!activeCard.isFlipped && activeCard.manaCost <= owningPlayer.currentMana)
                    c.HighlightCard();
            }
            else
            {
                if (!activeCard.hasSummoningSickness && !activeCard.isTapped && activeCard != c)
                    c.HighlightCard();
            }

        }
        List<GameObject> supportTargets = activeCard.supportAbility.GetComponentInChildren<HealAbility>().GetHighlightTargets();
        foreach (var t in supportTargets)
        {
            CardController c = t.GetComponentInChildren<CardController>();

            if (c == null || owningPlayer.name != "Player") continue;

            if (activeCard.isInHand)
            {
                if (!activeCard.isFlipped && activeCard.manaCost <= owningPlayer.currentMana)
                    c.HighlightCard();
            }
            else
            {
                if (!activeCard.hasSummoningSickness && !activeCard.isTapped && activeCard != c)
                    c.HighlightCard();
            }
        }
    }

    public void HideBoardTargets()
    {
        foreach (var t in GetAllBoardTargets())
        {
            CardController c = t.GetComponentInChildren<CardController>();
            if (c != null)
                c.UnHighlightCard();
        }
    }

    public void HidePlayableBoard()
    {
        foreach (var t in GetPlayerPlayableBoard())
        {
            CardController c = t.GetComponentInChildren<CardController>();
            if (c != null)
                c.UnHighlightCard();
        }
    }
    public void VisualizePlayableBoard()
    {

        foreach (var t in GetPlayerPlayableBoard())
        {
            CardController c = t.GetComponentInChildren<CardController>();
            if (c != null && !c.isTapped && !c.hasSummoningSickness)
                c.HighlightCard();
        }

    }

    public void VisualizePlayableHand()
    {
        foreach (var t in cardPositions)
        {
            CardController c = t.GetComponentInChildren<CardController>();
            if (c != null && c.manaCost <= owningPlayer.currentMana && owningPlayer.name == "Player")
            {
                if (!c.isFlipped || !AllPlayerSlotsFull())
                    c.HighlightCard();

            }

        }

    }
    public void HidePlayableHand()
    {

        foreach (var t in cardPositions)
        {
            CardController c = t.GetComponentInChildren<CardController>();
            if (c != null)
                c.UnHighlightCard();
        }

    }



    private void OnMouseDrag()
    {
        if (activeCard == null) return;


        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        lineRenderer.SetPosition(1, mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        SetLineColor(activeCard.isInPlay ? new Color(0.3f, 0.3f, 0.3f) : Color.white);
        if (hit.collider == null)
        {
            return;
        }

        GameObject hitObject = hit.collider.gameObject;


        string slotName = "PlayerSlot";
        if (owningPlayer.name == "Opponent")
            slotName = "OpponentSlot";


        if (activeCard.owningPlayer != encounterController.currentPlayer)
            return;

        if (hitObject.name.StartsWith(slotName))
        {
            show_addCardToEncounter(activeCard);
        }
        else if (hitObject.TryGetComponent(out CardController targetCard))
        {

            if (targetCard.owningPlayer == this.owningPlayer)
                show_useCardAbilityDefensive(targetCard);
            else
                show_useCardAbilityOffensive(targetCard);
        }
    }


    private void show_addCardToEncounter(CardController card)
    {
        if (activeCard.isInPlay) return;


        SetLineColor(activeCard.isInPlay ? new Color(0.3f, 0.3f, 0.3f) : Color.yellow);
    }

    private void addCardToEncounter(CardController card, GameObject hitObject)
    {
        if (activeCard.isInPlay) return;

        if (hitObject.GetComponentInChildren<CardController>() != null)
        {
            Debug.Log($"Cannot add {card.cardName} to {hitObject.name}, slot is already occupied.");
            return;
        }

        if (encounterController.currentPlayer.currentMana < card.manaCost)
        {
            Debug.Log($"Card {card.cardName} not added, not enough mana.");
            return;
        }

        encounterController.currentPlayer.SpendMana(card.manaCost);

        card.transform.SetParent(hitObject.transform);
        card.transform.position = hitObject.transform.position;
        card.transform.localPosition = Vector3.zero;

        card.isActive = false;
        card.isInHand = false;
        card.SetSummoningSickness(true);
        card.UnflipCard();
        card.EnterPlay();

        RemoveCardFromHand(card);
        encounterController.currentPlayer.AddCardToBoard(card);
    }

    // Defensive: CardController target
    private void show_useCardAbilityDefensive(CardController targetCard)
    {
        if (targetCard == activeCard || !targetCard.isInPlay) return;
        SetLineColor(activeCard.isInPlay ? Color.cyan : Color.green);
    }

    // Defensive: PlayerController target
    private void show_useCardAbilityDefensive(PlayerController targetPlayer)
    {
        SetLineColor(Color.green);
    }







    // Activate Defensive Ability: CardController target
    private void useCardAbilityDefensive(CardController targetCard)
    {
        if (!CanActivateAbility(targetCard)) return;

        activeCard.ActivateDefensiveAbility(targetCard);
        Debug.Log("Defense action triggered on card!");
    }

    // Activate Defensive Ability: PlayerController target
    private void useCardAbilityDefensive(PlayerController targetPlayer)
    {
        if (!CanActivateAbility(targetPlayer)) return;

        activeCard.ActivateDefensiveAbility(targetPlayer);
        Debug.Log("Defense action triggered on player!");
    }

    // Activate Offensive Ability: CardController target
    private void useCardAbilityOffensive(CardController targetCard)
    {
        if (!CanActivateAbility(targetCard)) return;

        activeCard.ActivateOffensiveAbility(targetCard);
        Debug.Log("Offense action triggered on card!");
    }

    // Activate Offensive Ability: PlayerController target
    private void useCardAbilityOffensive(PlayerController targetPlayer)
    {
        if (!CanActivateAbility(targetPlayer)) return;

        activeCard.ActivateOffensiveAbility(targetPlayer);
        Debug.Log("Offense action triggered on player!");
    }

    // Helper method to validate and prepare ability activation
    private bool CanActivateAbility(object target)
    {
        if (target == activeCard)
        {
            Debug.LogWarning("Cannot target the active card itself.");
            return false;
        }

        if (target is CardController targetCard && !targetCard.isInPlay)
        {
            Debug.LogWarning("Cannot target a card that is not in play.");
            return false;
        }

        if (activeCard.isInPlay && activeCard.hasSummoningSickness)
        {
            Debug.LogWarning("Cannot activate ability: Card has summoning sickness.");
            return false;
        }

        if (activeCard.isInPlay && activeCard.isTapped)
        {
            Debug.LogWarning("Cannot activate ability: Card is already tapped.");
            return false;
        }

        if (!activeCard.isInPlay && activeCard.isFlipped)
        {
            Debug.LogWarning("Cannot activate ability: Card is already flipped.");
            return false;
        }

        if (!activeCard.isInPlay && encounterController.currentPlayer.currentMana < activeCard.manaCost)
        {
            Debug.LogWarning($"Not enough mana to activate ability. Current Mana: {encounterController.currentPlayer.currentMana}, Required: {activeCard.manaCost}");
            return false;
        }

        // Example for checking target validity (expand as needed)
        // if (!activeCard.CheckAbilityTarget(target))
        // {
        //     Debug.LogWarning("Target is invalid for this ability.");
        //     return false;
        // }

        if (activeCard.isInPlay)
        {
            activeCard.TapCard();
        }
        else
        {
            activeCard.FlipCard();
            encounterController.currentPlayer.SpendMana(activeCard.manaCost);
        }

        return true;
    }





    // Offensive: CardController target
    private void show_useCardAbilityOffensive(CardController targetCard)
    {
        if (targetCard == activeCard || !targetCard.isInPlay) return;
        SetLineColor(activeCard.isInPlay ? new Color(1.0f, 0.5f, 0.0f) : Color.red);
    }

    // Offensive: PlayerController target
    private void show_useCardAbilityOffensive(PlayerController targetPlayer)
    {
        SetLineColor(Color.red);
    }


    private void SetLineColor(Color color)
    {
        lineRenderer.startColor = lineRenderer.endColor = color;
    }

    private void OnMouseUp()
    {
        if (activeCard == null) return;

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (encounterController.currentPlayer != owningPlayer)
        {
        }
        else if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;

            string slotName = "PlayerSlot";
            if (owningPlayer.name == "Opponent")
                slotName = "OpponentSlot";

            // drag card to empty slot
            if (hitObject.name.StartsWith(slotName))
                addCardToEncounter(activeCard, hitObject);

            // drag card on to another card
            else if (hitObject.TryGetComponent(out CardController targetCard))
            {
                if (targetCard.owningPlayer == this.owningPlayer)
                    useCardAbilityDefensive(targetCard);
                else
                    useCardAbilityOffensive(targetCard);
            }
            // drag card on to a player
            else if (hitObject.TryGetComponent(out PlayerController targetPlayer))
            {
                if (targetPlayer == owningPlayer)
                    useCardAbilityDefensive(targetPlayer);
                else
                    useCardAbilityOffensive(targetPlayer);
            }
        }

        //activeCard.GetComponent<HoverHandler>().enabled = true;
        activeCard = null;
        lineRenderer.enabled = false;

        if (encounterController.currentPlayer.name == "Player")
        {
            HideBoardTargets();
            VisualizePlayableHand();

            HidePlayableBoard();
            VisualizePlayableBoard();

        }
    }

    public List<CardController> GetHand() => playerHand;

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

}