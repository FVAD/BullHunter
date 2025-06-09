using UnityEngine;

public class UIAudioPlayer : MonoBehaviour
{
    public void PlayPopup() => AudioMap.UI.Popup.Play();
    public void PlayPoint() => AudioMap.UI.Point.Play();
    public void PlayClick() => AudioMap.UI.Click.Play();
}
