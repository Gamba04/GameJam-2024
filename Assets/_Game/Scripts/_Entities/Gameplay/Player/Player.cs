using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
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

    [Header("Detection")]
    [SerializeField]
    private LayerMask worldDetection;
    [SerializeField]
    private LayerMask playerDetection;

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
    private float jump;
    [SerializeField]
    private float knockback;

    [Space]
    [SerializeField]
    [Range(-1, 1)]
    private float groundedThreshold = -0.6f;
    [SerializeField]
    private float frictionThreshold = 0.1f;

    [Header("Info")]
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
    }

    private void InitEvents()
    {
        input.onMovement += Move;
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

        if (hasCigarette && !hasCigaretteCooldown)
        {
            SetCigarette(false);
            player.SetCigarette(true);
        }
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Mechanics

    private void Move(float input)
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

    private void Jump()
    {
        if (!isGrounded) return;

        rb.velocity = new Vector2(rb.velocity.x, jump);
    }

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
    }

#endif

    #endregion

}