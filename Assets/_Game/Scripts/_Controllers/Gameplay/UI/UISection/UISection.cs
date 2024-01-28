using UnityEngine;

public abstract class UISection : MonoBehaviour
{
    public abstract void SetVisible(bool value, bool instant = false);
}