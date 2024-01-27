using System;
using System.Collections.Generic;
using UnityEngine;

public enum InputState
{
    Start,
    Stay,
    End,
    Canceled
}

#region Serializable

[Serializable]
public class GameTouch
{
    [SerializeField, HideInInspector] private string name;

    public int id;
    public Vector2 screenPos;
    public InputState state;
    public Vector2 deltaPos;
    public List<Collider2D> originColliders = new List<Collider2D>();
    public List<Collider2D> activeColliders = new List<Collider2D>();
    public List<Collider2D> allInteractedColliders = new List<Collider2D>();

    public GameTouch(int id, Vector2 screenPos, InputState state, Vector2 deltaPos)
    {
        this.id = id;
        this.screenPos = screenPos;
        this.state = state;
        this.deltaPos = deltaPos;

#if UNITY_EDITOR
        SetName();
#endif
    }

    public void UpdateInput(Vector2 screenPos, InputState state, Vector2 deltaPos)
    {
        this.screenPos = screenPos;
        this.state = state;
        this.deltaPos = deltaPos;
    }

    public bool IsFromArea(Collider2D collider) => originColliders.Contains(collider);

    public bool IsOnArea(Collider2D collider) => activeColliders.Contains(collider);

    public bool HasBeenOnArea(Collider2D collider) => allInteractedColliders.Contains(collider);

    public bool IsTouchingLayer(int layer) => activeColliders.Find(f => f.gameObject.layer == layer) != null;

    private void SetName() => name = id > -1 ? $"Touch {id}" : "Mouse Touch";
}

#endregion

public class TouchManager : MonoBehaviour
{
    [SerializeField]
    private bool useWorldDetection = true;
    [SerializeField]
    private LayerMask worldDetection;

    [Space]
    [SerializeField]
    private bool useUIDetection = true;
    [SerializeField]
    private LayerMask uiDetection;
    [SerializeField]
    private bool useMouseTouch = true;

    [Space]
    [SerializeField]
    private bool useMaxTouches = false;
    [SerializeField]
    private int maxTouches = 10;

    [Space]
    [SerializeField]
    private List<GameTouch> touches = new List<GameTouch>();

    private Vector2 lastMousePosition;

    public static List<GameTouch> Touches => Instance.touches;

    public static bool UseMouseTouch { get => Instance.useMouseTouch; set => Instance.useMouseTouch = value; }

    #region Singleton

    private static TouchManager instance = null;

    public static TouchManager Instance => GambaFunctions.GetSingleton(ref instance);

    private void Awake() => GambaFunctions.OnSingletonInit(ref instance, this);

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Update

    private void Update()
    {
        UpdateTouches();

        if (useWorldDetection || useUIDetection)
        {
            ColliderDetection();
            SendTouchInfo();
        }
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region InputDetection

    private void UpdateTouches()
    {
        CleanList();

        if (Input.mousePresent && useMouseTouch)
        {
            GameTouch input = GetInputInList(-1);
            Vector2 delta = ((Vector2)Input.mousePosition - lastMousePosition);

            if (Input.GetMouseButton(0))
            {
                if (Input.GetMouseButtonDown(0)) // Mouse Enter ------------------------------------------------------------
                {
                    if (input != null)
                    {
                        input.UpdateInput(Input.mousePosition, InputState.Start, delta);
                    }
                    else
                    {
                        touches.Add(new GameTouch(-1, Input.mousePosition, InputState.Start, delta));
                    }
                }
                else // Mouse Stay ------------------------------------------------------------
                {
                    if (input != null)
                    {
                        input.UpdateInput(Input.mousePosition, InputState.Stay, delta);
                    }
                    else
                    {
                        touches.Add(new GameTouch(-1, Input.mousePosition, InputState.Stay, delta));
                    }
                }

                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0)) // Mouse Exit ------------------------------------------------------------
            {
                if (input != null)
                {
                    input.UpdateInput(Input.mousePosition, InputState.End, delta);
                }
            }
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            GameTouch input = GetInputInList(touch.fingerId);

            if (touch.phase == TouchPhase.Began) // Touch Enter ------------------------------------------------------------
            {
                if (input != null)
                {
                    input.UpdateInput(touch.position, InputState.Start, touch.deltaPosition);
                }
                else
                {
                    touches.Add(new GameTouch(touch.fingerId, touch.position, InputState.Start, touch.deltaPosition));
                }
            }
            else if (touch.phase == TouchPhase.Ended) // Touch Exit ------------------------------------------------------------
            {
                if (input != null)
                {
                    input.UpdateInput(touch.position, InputState.End, touch.deltaPosition);
                }
            }
            else if (touch.phase == TouchPhase.Canceled) // Touch Canceled ------------------------------------------------------------
            {
                if (input != null)
                {
                    input.UpdateInput(touch.position, InputState.Canceled, touch.deltaPosition);
                }
            }
            else // Touch Stay ------------------------------------------------------------
            {
                if (input != null)
                {
                    input.UpdateInput(touch.position, InputState.Stay, touch.deltaPosition);
                }
                else
                {
                    touches.Add(new GameTouch(touch.fingerId, touch.position, InputState.Stay, touch.deltaPosition));
                }
            }
        }

        if (touches.Count > maxTouches && useMaxTouches)
        {
            touches.ResizeEmpty(maxTouches);
        }
    }

    private void CleanList()
    {
        for (int i = 0; i < touches.Count; i++)
        {
            if (touches[i].state == InputState.End || touches[i].state == InputState.Canceled)
            {
                touches.RemoveAt(i);
            }
        }
    }

    private GameTouch GetInputInList(int id)
    {
        GameTouch input = null;

        for (int i = 0; i < touches.Count; i++)
        {
            if (touches[i].id == id)
            {
                input = touches[i];
                break;
            }
        }
        return input;
    }


    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region ColliderDetection

    private void ColliderDetection()
    {
        if (Camera.main != null)
        {
            for (int i = 0; i < touches.Count; i++)
            {
                GameTouch touch = touches[i];

                // World Detection
                List<Collider2D> colliders = useWorldDetection ? new List<Collider2D>(Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(touch.screenPos), worldDetection)) : new List<Collider2D>();

                // UI Detection
                List<Collider2D> uiColliders = useUIDetection ? new List<Collider2D>(Physics2D.OverlapPointAll(UIManager.ScreenToCanvasPos(touch.screenPos), uiDetection)) : new List<Collider2D>();

                colliders.AddRange(uiColliders);

                // Set colliders info
                if (touch.state == InputState.Start) touch.originColliders = colliders;
                touch.activeColliders = colliders;

                for (int o = 0; o < colliders.Count; o++)
                {
                    Collider2D area = colliders[o];

                    if (!touch.HasBeenOnArea(area))
                    {
                        touch.allInteractedColliders.Add(area);
                    }
                }
            }
        }
    }

    private void SendTouchInfo()
    {
        for (int i = 0; i < touches.Count; i++)
        {
            GameTouch touch = touches[i];

            for (int o = 0; o < touch.allInteractedColliders.Count; o++)
            {
                Collider2D area = touch.allInteractedColliders[o];

                ITouchInteractable obj = CheckForInteractable(area);

                if (obj != null)
                {
                    obj.OnTouch(touch, touch.IsOnArea(area), touch.IsFromArea(area));
                }
            }
        }
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Static Methods

    public static bool Exists(GameTouch input) => Instance.touches.Contains(input);

    public static bool IsTouchingTag(InteractionTag tag)
    {
        for (int i = 0; i < Instance.touches.Count; i++)
        {
            for (int c = 0; c < Instance.touches[i].activeColliders.Count; c++)
            {
                ITouchInteractable obj = CheckForInteractable(Instance.touches[i].activeColliders[c]);

                if (obj != null)
                {
                    if (obj.Tag == tag) return true;
                }
            }
        }

        return false;
    }

    public static bool IsTouchingTag(string tag)
    {
        for (int i = 0; i < Instance.touches.Count; i++)
        {
            for (int c = 0; c < Instance.touches[i].activeColliders.Count; c++)
            {
                if (Instance.touches[i].activeColliders[c].gameObject.CompareTag(tag)) return true;
            }
        }

        return false;
    }

    public static bool IsTouchingLayers(params int[] layers)
    {
        for (int i = 0; i < Instance.touches.Count; i++)
        {
            for (int c = 0; c < Instance.touches[i].activeColliders.Count; c++)
            {
                int currentLayer = Instance.touches[i].activeColliders[c].gameObject.layer;

                foreach (int layer in layers) if (currentLayer == layer) return true;
            }
        }

        return false;
    }

    private static ITouchInteractable CheckForInteractable(Collider2D collider)
    {
        ITouchInteractable target = collider.attachedRigidbody?.GetComponent<ITouchInteractable>();

        if (target == null) target = collider.GetComponentInParent<ITouchInteractable>();

        return target;
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;

        for (int i = 0; i < touches.Count; i++)
        {
            if (Camera.main != null) Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(touches[i].screenPos), 0.5f);

            Gizmos.DrawSphere(UIManager.ScreenToCanvasPos(touches[i].screenPos), 0.5f);
        }
    }

#endif

    #endregion

}