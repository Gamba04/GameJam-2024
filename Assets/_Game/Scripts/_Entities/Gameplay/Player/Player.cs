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
    private LayerMask worldDetection;

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

    [Space]
    [SerializeField]
    [Range(-1, 0)]
    private float groundedThreshold = -0.6f;
    [SerializeField]
    private float frictionThreshold = 0.1f;

    [Header("Info")]
    [ReadOnly, SerializeField]
    private bool isGrounded;

    private float Friction => isGrounded ? groundFriction : airFriction;

    #region Init

    public void Init()
    {
        InitEvents();
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

        ProcessWorldCollisions(contacts.FindAll(contact => worldDetection.Contains(contact.collider.gameObject.layer)));
    }

    private void ProcessWorldCollisions(List<ContactPoint2D> contacts)
    {
        float minHeight = 1;

        contacts.ForEach(ProcessCollision);

        isGrounded = minHeight <= groundedThreshold;

        void ProcessCollision(ContactPoint2D contact)
        {
            Vector2 vector = contact.point - (Vector2)collider.transform.position;

            UnityEngine.Debug.DrawRay(collider.transform.position, vector, Color.red);

            float heightValue = Vector2.Dot(vector.normalized, Vector2.up);
            minHeight = Math.Min(minHeight, heightValue);
        }
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Mechanics

    #region Movement

    private void Move(float input)
    {
        float velocity = rb.velocity.x;
        float inputAmount = Mathf.Abs(input);

        ApplyAcceleration();
        ApplyFriction();
        LimitSpeed();

        rb.velocity = new Vector2(velocity, rb.velocity.y);

        SetVisualsDirection();

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

        void SetVisualsDirection()
        {
            if (inputAmount == 0) return;

            visuals.SetDirection(Mathf.Sign(input), inputAmount);
        }
    }

    private void Jump()
    {
        if (!isGrounded) return;

        rb.velocity = new Vector2(rb.velocity.x, jump);
    }

    #endregion

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
        RestrictNegatives(ref speed);
        RestrictNegatives(ref acceleration);
        RestrictNegatives(ref groundFriction);
        RestrictNegatives(ref airFriction);
        RestrictNegatives(ref jump);
        RestrictNegatives(ref frictionThreshold);

        void RestrictNegatives(ref float value) => value = Mathf.Max(value, 0);
    }

#endif

    #endregion

}