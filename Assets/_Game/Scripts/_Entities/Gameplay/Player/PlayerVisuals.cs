using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Transform graphics;
    [SerializeField]
    private GameObject cigarette;

    private float direction;

    private const float directionSpeed = 10;

    #region Public Methods

    public void SetDirection(float targetDirection, float inputAmount = 1)
    {
        direction = Mathf.Lerp(direction, targetDirection, inputAmount == 1 ? 1 : Time.deltaTime * directionSpeed * inputAmount);

        float x = direction > 0 ? 1 : -1;

        graphics.localScale = new Vector3(x, 1, 1);
    }

    public void SetCigarette(bool value)
    {
        cigarette.SetActive(value);
    }

    public void SetMoving(bool value)
    {
        anim.SetBool("Moving", value);
    }

    public void SetGrounded(bool value)
    {
        anim.SetBool("Grounded", value);
    }

    public void SetWallRiding(bool value)
    {
        anim.SetBool("WallRiding", value);
    }

    public void SetBall(bool value)
    {
        anim.SetBool("Ball", value);
    }

    #endregion

}