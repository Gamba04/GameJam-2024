using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private SpriteRenderer sprite;
    [SerializeField]
    private SpriteRenderer cigarette;

    private float direction;

    private const float directionSpeed = 10;

    #region Public Methods

    public void SetDirection(float targetDirection, float inputAmount)
    {
        direction = Mathf.Lerp(direction, targetDirection, inputAmount == 1 ? 1 : Time.deltaTime * directionSpeed * inputAmount);

        sprite.flipX = direction < 0;
    }

    public void SetCigarette(bool value)
    {
        cigarette.enabled = value;
    }

    public void SetMoving(bool value)
    {
        anim.SetBool("Moving", value);
    }

    public void SetGrounded(bool value)
    {
        anim.SetBool("Grounded", value);
    }

    #endregion

}