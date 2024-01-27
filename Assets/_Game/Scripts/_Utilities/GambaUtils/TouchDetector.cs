using UnityEngine;

public class TouchDetector : MonoBehaviour, ITouchInteractable
{
    public delegate void TouchEvent(GameTouch touch, bool isOnArea, bool beganOnArea);

    public event TouchEvent onTouch;

    InteractionTag ITouchInteractable.Tag => InteractionTag.Default;

    void ITouchInteractable.OnTouch(GameTouch touch, bool isOnArea, bool beganOnArea)
    {
        onTouch?.Invoke(touch, isOnArea, beganOnArea);
    }
}