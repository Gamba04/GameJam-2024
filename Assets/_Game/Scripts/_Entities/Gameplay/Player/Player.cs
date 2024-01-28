using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    #region Custom Data

    private enum State
    {
        Idle,
        Normal,
        Ball
    }

    #endregion

    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private new CircleCollider2D collider;
    [SerializeField]
    private PlayerInput input;
    [SerializeField]
    private PlayerVisuals visuals;

    [Header("Settings")]
    [SerializeField]
    private float cigaretteCooldown;

    [Header("Movement")]
    [SerializeField]
    private float speed;
    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float groundFriction;
    [SerializeField]
    private float wallFriction;
    [SerializeField]
    private float airFriction;
    [SerializeField]
    private float frictionThreshold = 0.1f;
    [SerializeField]
    private float jump;
    [SerializeField]
    private float knockback;

    [Header("Ball")]
    [SerializeField]
    private float ballAcceleration;
    [SerializeField]
    private float ballSpeed;
    [SerializeField]
    private float ballJump;

    [Header("Collisions")]
    [SerializeField]
    private LayerMask worldDetection;
    [SerializeField]
    private LayerMask playerDetection;

    [Space]
    [SerializeField]
    private PhysicsMaterial2D normalMaterial;
    [SerializeField]
    private PhysicsMaterial2D ballMaterial;

    [Space]
    [SerializeField]
    private float normalColliderSize;
    [SerializeField]
    private float normalColliderOffset;
    [SerializeField]
    private float ballColliderSize;
    [SerializeField]
    private float ballColliderOffset;

    [Space]
    [SerializeField]
    [Range(-1, 1)]
    private float groundedThreshold = -0.6f;
    [SerializeField]
    [Range(-1, 1)]
    private float wallTargetValue;
    [SerializeField]
    [Range(0, 0.5f)]
    private float wallValueRange;
    [SerializeField]
    private float groundedDropTimeout;
    [SerializeField]
    private float wallRideDropTimeout;

    [Header("Audio")]
    [SerializeField]
    private List<SFXTag> jumpSFX;

    [Header("Info")]
    [ReadOnly, SerializeField]
    private State state;
    [ReadOnly, SerializeField]
    private bool isGrounded;
    [ReadOnly, SerializeField]
    private bool isWallRiding;
    [ReadOnly, SerializeField]
    private int wallDirection;
    [ReadOnly, SerializeField]
    private bool hasCigarette;
    [ReadOnly, SerializeField]
    private bool hasCigaretteCooldown;

    private int playerID;

    private bool hasJumpCooldown;

    private Vector2 levelArea;

    private Timer.CancelRequest cancelGrounded = new Timer.CancelRequest();
    private Timer.CancelRequest cancelWallDrop = new Timer.CancelRequest();

    public event Action onCigarette;

    private float Friction => isGrounded ? groundFriction : airFriction;

    public int PlayerID => playerID;

    #region Init

    public void Init(int playerID, Vector2 levelArea)
    {
        this.playerID = playerID;
        this.levelArea = levelArea;

        InitEvents();

        input.Init(playerID);

        SetBall(false);
        SetCigarette(false);

        SetState(State.Idle);
    }

    private void InitEvents()
    {
        input.onMovement += Movement;
        input.onJump += Jump;
        input.onBall += ToggleBall;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region FixedUpdate

    private void FixedUpdate()
    {
        UpdateCollisions();
        UpdateWallRide();
        UpdateMirror();
    }

    #region Collisions

    private void UpdateCollisions()
    {
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        collider.GetContacts(contacts);

        ProcessWorldCollisions(contacts.FindAll(contact => worldDetection.Contains(GetLayer(contact))));
        ProcessPlayerCollision(contacts.Find(contact => playerDetection.Contains(GetLayer(contact))));

        int GetLayer(ContactPoint2D contact) => contact.collider.gameObject.layer;
    }

    private void ProcessWorldCollisions(List<ContactPoint2D> contacts)
    {
        float minX = 1;
        float maxX = -1;
        float minY = 1;

        contacts.ForEach(ProcessCollisionValues);

        SetGrounded(minY <= groundedThreshold);
        SetWallRiding(CheckWallRiding(out int direction), direction);

        void ProcessCollisionValues(ContactPoint2D contact)
        {
            Vector2 direction = -contact.normal;

            UnityEngine.Debug.DrawRay(collider.transform.position, direction * 0.5f, Color.red);

            float xValue = Vector2.Dot(direction, Vector2.right);
            float yValue = Vector2.Dot(direction, Vector2.up);

            minX = Mathf.Min(minX, xValue);
            maxX = Mathf.Max(maxX, xValue);
            minY = Mathf.Min(minY, yValue);
        }

        bool CheckWallRiding(out int direction)
        {
            direction = 0;

            float yMinLimit = wallTargetValue - wallValueRange;
            float yMaxLimit = wallTargetValue + wallValueRange;

            if (minY > yMaxLimit || minY < yMinLimit) return false;

            int minDir = GetWallDir(minX);
            int maxDir = GetWallDir(maxX);

            if (minDir != maxDir) return false;

            direction = minDir;
            return true;
        }

        int GetWallDir(float value) => value > 0 ? 1 : -1;
    }

    private void ProcessPlayerCollision(ContactPoint2D contact)
    {
        if (contact.collider == null) return;

        Player player = contact.collider.GetComponentInParent<Player>();

        if (player == null) return;

        Vector2 direction = -contact.normal;

        player.Knockback(direction);

        if (hasCigarette && !hasCigaretteCooldown && state != State.Idle)
        {
            SetCigarette(false);
            player.SetCigarette(true);
        }
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void UpdateWallRide()
    {
        if (state != State.Normal || !isWallRiding) return;

        float velocity = rb.velocity.y;

        if (velocity < 0)
        {
            velocity /= 1 + wallFriction * Time.deltaTime;
        }

        rb.velocity = new Vector2(rb.velocity.x, velocity);
    }

    private void UpdateMirror()
    {
        Vector2 position = transform.position;

        if (position.x >  levelArea.x) position.x = -levelArea.x;
        if (position.x < -levelArea.x) position.x =  levelArea.x;
        if (position.y >  levelArea.y) position.y = -levelArea.y;
        if (position.y < -levelArea.y) position.y =  levelArea.y;

        transform.position = position;
    }

    #endregion

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Mechanics

    private void Movement(float input)
    {
        Action<float> movement = state switch
        {
            State.Normal => NormalMovement,
            State.Ball => BallMovement,
            _ => null
        };

        movement?.Invoke(input);
    }

    private void Jump()
    {
        if (hasJumpCooldown) return;

        hasJumpCooldown = true;
        Timer.CallOnDelay(OnFinishCooldown, Mathf.Max(groundedDropTimeout, wallRideDropTimeout));

        Action jump = state switch
        {
            State.Normal => NormalJump,
            State.Ball => BallJump,
            _ => null
        };

        jump?.Invoke();

        void OnFinishCooldown()
        {
            hasJumpCooldown = false;
        }
    }

    private void ToggleBall()
    {
        switch (state)
        {
            case State.Normal:
                SetBall(true);
                break;

            case State.Ball:
                SetBall(false);
                break;
        }
    }

    private void SetBall(bool value)
    {
        SetState(value ? State.Ball : State.Normal);

        visuals.SetBall(value);

        collider.radius = value ? ballColliderSize : normalColliderSize;
        collider.offset = Vector2.up * (value ? ballColliderOffset : normalColliderOffset);
        collider.sharedMaterial = value ? ballMaterial : normalMaterial;
        rb.constraints = value ? RigidbodyConstraints2D.None : RigidbodyConstraints2D.FreezeRotation;

        if (!value)
        {
            rb.rotation = 0;
        }
    }

    #region Normal

    private void NormalMovement(float input)
    {
        float velocity = rb.velocity.x;
        float inputAmount = Mathf.Abs(input);

        ApplyAcceleration();
        ApplyFriction();
        LimitSpeed();

        rb.velocity = new Vector2(velocity, rb.velocity.y);

        SetVisuals();

        void ApplyAcceleration()
        {
            velocity += input * acceleration * Time.deltaTime;
        }

        void ApplyFriction()
        {
            float inputCoefficient = 1 - inputAmount;

            velocity /= 1 + Friction * inputCoefficient * Time.deltaTime;

            if (GetMagnitude() <= frictionThreshold) velocity = 0;
        }

        void LimitSpeed()
        {
            velocity = Mathf.Min(GetMagnitude(), speed) * GetDirection();
        }

        float GetMagnitude() => Mathf.Abs(velocity);

        float GetDirection() => Mathf.Sign(velocity);

        void SetVisuals()
        {
            if (inputAmount > 0)
            {
                visuals.SetMoving(true);
                visuals.SetDirection(Mathf.Sign(input), inputAmount);
            }
            else
            {
                visuals.SetMoving(false);
            }
        }
    }

    private void NormalJump()
    {
        if (isWallRiding)
        {
            int jumpDirection = -wallDirection;

            visuals.SetDirection(jumpDirection);

            rb.velocity = new Vector2(jumpDirection * jump, jump);
        }
        else if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jump);
        }

        SFXPlayer.PlayRandomSFX(jumpSFX);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Ball

    private void BallMovement(float input)
    {
        float velocity = rb.velocity.x;

        ApplyAcceleration();
        LimitSpeed();

        rb.velocity = new Vector2(velocity, rb.velocity.y);

        void ApplyAcceleration()
        {
            velocity += input * ballAcceleration * Time.deltaTime;
        }

        void LimitSpeed()
        {
            velocity = Mathf.Min(GetMagnitude(), ballSpeed) * GetDirection();
        }

        float GetMagnitude() => Mathf.Abs(velocity);

        float GetDirection() => Mathf.Sign(velocity);
    }

    private void BallJump()
    {
        if (!isGrounded) return;

        rb.velocity = new Vector2(rb.velocity.x, ballJump);

        SFXPlayer.PlayRandomSFX(jumpSFX);
    }

    #endregion

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void OnStartGame(int startingPlayer)
    {
        SetState(State.Normal);
        SetCigarette(playerID == startingPlayer);
    }

    public void SetCigarette(bool value)
    {
        hasCigarette = value;

        visuals.SetCigarette(value);

        if (value)
        {
            OnCigarette();
        }
    }

    public void Knockback(Vector2 direction)
    {
        rb.velocity = direction * knockback;
    }

    public void GameOver()
    {
        SetState(State.Idle);

        Vector2 direction = new Vector2(GetRandomValue(), GetRandomValue()).normalized;

        Knockback(direction);

        float GetRandomValue() => UnityEngine.Random.Range(-1f, 1f);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void SetGrounded(bool value)
    {
        if (value != isGrounded)
        {
            visuals.SetGrounded(value);

            if (value)
            {
                ApplyChange();

                cancelGrounded.Cancel();
            }
            else
            {
                Timer.CallOnDelay(ApplyChange, groundedDropTimeout, cancelGrounded);
            }
        }

        void ApplyChange()
        {
            isGrounded = value;
        }
    }

    private void SetWallRiding(bool value, int direction)
    {
        if (value != isWallRiding)
        {
            visuals.SetWallRiding(value);

            if (value)
            {
                ApplyChange();

                cancelWallDrop.Cancel();
            }
            else
            {
                Timer.CallOnDelay(ApplyChange, wallRideDropTimeout, cancelWallDrop);
            }
        }

        void ApplyChange()
        {
            isWallRiding = value;
            wallDirection = direction;
        }
    }

    private void OnCigarette()
    {
        SetCigarretteCooldown();

        onCigarette?.Invoke();
    }

    private void SetCigarretteCooldown()
    {
        hasCigaretteCooldown = true;

        Timer.CallOnDelay(OnFinishCooldown, cigaretteCooldown);

        void OnFinishCooldown()
        {
            hasCigaretteCooldown = false;
        }
    }

    private void SetState(State state)
    {
        this.state = state;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        UpdateEditorFields();   
    }

    private void UpdateEditorFields()
    {
        GambaFunctions.RestrictNegativeValues(ref cigaretteCooldown);
        GambaFunctions.RestrictNegativeValues(ref speed);
        GambaFunctions.RestrictNegativeValues(ref acceleration);
        GambaFunctions.RestrictNegativeValues(ref groundFriction);
        GambaFunctions.RestrictNegativeValues(ref airFriction);
        GambaFunctions.RestrictNegativeValues(ref jump);
        GambaFunctions.RestrictNegativeValues(ref frictionThreshold);
        GambaFunctions.RestrictNegativeValues(ref knockback);
        GambaFunctions.RestrictNegativeValues(ref wallFriction);
        GambaFunctions.RestrictNegativeValues(ref groundedDropTimeout);
        GambaFunctions.RestrictNegativeValues(ref wallRideDropTimeout);
        GambaFunctions.RestrictNegativeValues(ref ballAcceleration);
        GambaFunctions.RestrictNegativeValues(ref ballSpeed);
        GambaFunctions.RestrictNegativeValues(ref ballJump);
        GambaFunctions.RestrictNegativeValues(ref normalColliderSize);
        GambaFunctions.RestrictNegativeValues(ref ballColliderSize);
    }

#endif

    #endregion

}