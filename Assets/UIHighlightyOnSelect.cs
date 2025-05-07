using UnityEngine;
using UnityEngine.EventSystems;

public class UIHighlightOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject highlightImage;

    private void Awake()
    {
        if (highlightImage != null)
            highlightImage.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (highlightImage != null)
            highlightImage.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (highlightImage != null)
            highlightImage.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightImage != null)
            highlightImage.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightImage != null)
            highlightImage.SetActive(false);
    }
}
