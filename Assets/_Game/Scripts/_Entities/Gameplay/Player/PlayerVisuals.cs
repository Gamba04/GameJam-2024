using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private SpriteRenderer sprite;

    private float direction;

    private const float directionSpeed = 10;

    #region Public Methods

    public void SetDirection(float targetDirection, float inputAmount)
    {
        direction = Mathf.Lerp(direction, targetDirection, inputAmount == 1 ? 1 : Time.deltaTime * directionSpeed * inputAmount);

        sprite.flipX = direction < 0;
    }

    #endregion

}