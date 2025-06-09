using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTip : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData _) => AudioMap.UI.Point.Play();
    public void OnPointerClick(PointerEventData _) => AudioMap.UI.Click.Play();
}
