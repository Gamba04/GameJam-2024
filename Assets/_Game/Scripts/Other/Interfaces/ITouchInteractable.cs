/// <summary> Setup custom interaction tags to filter by touch's contact with a tag. </summary>
public enum InteractionTag
{
    Default,
}

public interface ITouchInteractable
{
    /// <summary> Information about an existing touch that has had contact with this Collider2D. </summary>
    /// <param name="isOnArea"> If the touch is actively touching this Collder2D. </param>
    /// <param name="beganOnArea"> If the touch started while touching this Collider2D. </param>
    void OnTouch(GameTouch touch, bool isOnArea, bool beganOnArea);

    InteractionTag Tag { get; }
}