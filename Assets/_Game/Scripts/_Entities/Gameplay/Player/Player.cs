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
    }

    #endregion

    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private new Collider2D collider;
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
    private float airFriction;
    [SerializeField]
    private float frictionThreshold = 0.1f;
    [SerializeField]
    private float jump;
    [SerializeField]
    private float knockback;

    [Header("Collisions")]
    [SerializeField]
    private LayerMask worldDetection;
    [SerializeField]
    private LayerMask playerDetection;

    [Space]
    [SerializeField]
    [Range(-1, 1)]
    private float groundedThreshold = -0.6f;
    [SerializeField]
    [Range(-1, 1)]
    private float wallTargetValue;
    [SerializeField]
    private float wallValueRange;

    [Header("Info")]
    [ReadOnly, SerializeField]
    private State state;
    [ReadOnly, SerializeField]
    private bool isGrounded;
    [ReadOnly, SerializeField]
    private bool hasCigarette;
    [ReadOnly, SerializeField]
    private bool hasCigaretteCooldown;

    private int playerID;

    public event Action onCigarette;

    private float Friction => isGrounded ? groundFriction : airFriction;

    public int PlayerID => playerID;

    #region Init

    public void Init(int playerID)
    {
        this.playerID = playerID;

        InitEvents();

        input.Init(playerID);

        SetCigarette(false);

        SetState(State.Normal);
    }

    private void InitEvents()
    {
        input.onMovement += Movement;
        input.onJump += Jump;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Collisions

    private void FixedUpdate()
    {
        UpdateCollisions();
    }

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
        float minHeight = 1;

        contacts.ForEach(ProcessCollision);

        SetGrounded(minHeight <= groundedThreshold);

        void ProcessCollision(ContactPoint2D contact)
        {
            Vector2 direction = -contact.normal;

            UnityEngine.Debug.DrawRay(collider.transform.position, direction * 0.5f, Color.red);

            float heightValue = Vector2.Dot(direction, Vector2.up);
            minHeight = Math.Min(minHeight, heightValue);
        }
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

    #region Mechanics

    private void Movement(float input)
    {
        Action<float> movement = state switch
        {
            State.Normal => NormalMovement,
            _ => null
        };

        movement?.Invoke(input);
    }

    private void Jump()
    {
        Action jump = state switch
        {
            State.Normal => NormalJump,
            _ => null
        };

        jump?.Invoke();
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
        if (!isGrounded) return;

        rb.velocity = new Vector2(rb.velocity.x, this.jump);
    }

    #endregion

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

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
        isGrounded = value;

        visuals.SetGrounded(value);
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
        GambaFunctions.RestrictNegativeValues(ref wallValueRange);
    }

#endif

    #endregion

}