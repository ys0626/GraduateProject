using UnityEngine;
using UnityEngine.EventSystems;

public class PopupBackground :
    MonoBehaviour,
    IPointerClickHandler
{
    public GameObject popup;

    public void OnPointerClick(PointerEventData eventData)
    {
        popup.SetActive(false);
    }
}