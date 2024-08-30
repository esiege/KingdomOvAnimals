using UnityEngine;
using UnityEngine.EventSystems;

public class HoverableCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;

    public void SetHoverEffect()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * 1.1f; // Enlarge slightly on hover
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale; // Reset scale when no longer hovering
    }
}
