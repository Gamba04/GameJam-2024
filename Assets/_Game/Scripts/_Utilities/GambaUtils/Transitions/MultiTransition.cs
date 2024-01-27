using System;
using UnityEngine;
using Internal;

#region MultiTransition x2

[Serializable]
public class MultiTransition<A, B>
    where A : struct
    where B : struct
{
    public float duration = 1;

    [Space]
    public AnimationCurve aCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private A aValue;

    [Space]
    public AnimationCurve bCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private B bValue;

    [Space]
    [SerializeField]
    private bool isOnTransition;

    private Transition<A> aTransition = new Transition<A>();
    private Transition<B> bTransition = new Transition<B>();

    public A AValue { get => aTransition.value; set => aTransition.value = value; }
    public B BValue { get => bTransition.value; set => bTransition.value = value; }

    public bool IsOnTransition => aTransition.IsOnTransition || bTransition.IsOnTransition;

    #region Public Methods

    public void SetValues(A aValue, B bValue)
    {
        AValue = aValue;
        BValue = bValue;
    }

    public void StartTransition(A aValue, B bValue, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, false, false, false, onTransitionEnd);

    public void StartTransition(A aValue, B bValue, bool inverseCurve, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, false, inverseCurve, false, onTransitionEnd);

    public void StartTransition(A aValue, B bValue, bool inverseCurve, bool scaleDuration, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, false, inverseCurve, scaleDuration, onTransitionEnd);

    public void StartTransitionUnscaled(A aValue, B bValue, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, true, false, false, onTransitionEnd);

    public void StartTransitionUnscaled(A aValue, B bValue, bool inverseCurve, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, true, inverseCurve, false, onTransitionEnd);

    public void StartTransitionUnscaled(A aValue, B bValue, bool inverseCurve, bool scaleDuration, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, true, inverseCurve, scaleDuration, onTransitionEnd);

    /// <summary> Updates transition value when a transition is ocurring. </summary>
    /// <param name="onTransitionUpdate"> Callback to execute when a transition is ocurring. </param>
    public void UpdateTransition(Action<A, B> onTransitionUpdate = null)
    {
        aTransition.UpdateTransition();
        bTransition.UpdateTransition();

        isOnTransition = IsOnTransition;
        aValue = AValue;
        bValue = BValue;

        if (IsOnTransition) onTransitionUpdate?.Invoke(aValue, bValue);
    }

    /// <summary> End transition and call onTransitionEnd. </summary>
    public void EndTransition()
    {
        aTransition.EndTransition();
        bTransition.EndTransition();

        isOnTransition = IsOnTransition;
        aValue = AValue;
        bValue = BValue;
    }

    /// <summary> End transition without calling onTransitionEnd. </summary>
    public void Cancel()
    {
        aTransition.Cancel();
        bTransition.Cancel();

        isOnTransition = IsOnTransition;
        aValue = AValue;
        bValue = BValue;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void StartTransitionInternal(A aValue, B bValue, bool unscaled, bool inverseCurve, bool scaleDuration, Action onTransitionEnd)
    {
        // Setup durations
        aTransition.duration = duration;
        bTransition.duration = duration;

        // Setup curves
        aTransition.curve = aCurve;
        bTransition.curve = bCurve;

        // Launch Transitions
        if (unscaled)
        {
            aTransition.StartTransitionUnscaled(aValue, inverseCurve, scaleDuration, onTransitionEnd);
            bTransition.StartTransitionUnscaled(bValue, inverseCurve, scaleDuration);
        }
        else
        {
            aTransition.StartTransition(aValue, inverseCurve, scaleDuration, onTransitionEnd);
            bTransition.StartTransition(bValue, inverseCurve, scaleDuration);
        }

        isOnTransition = IsOnTransition;
    }

    #endregion

}

#endregion

#region MultiTransition x3

[Serializable]
public class MultiTransition<A, B, C>
    where A : struct
    where B : struct
    where C : struct
{
    public float duration = 1;

    [Space]
    public AnimationCurve aCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private A aValue;

    [Space]
    public AnimationCurve bCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private B bValue;

    [Space]
    public AnimationCurve cCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private C cValue;

    [Space]
    [SerializeField]
    private bool isOnTransition;

    private Transition<A> aTransition = new Transition<A>();
    private Transition<B> bTransition = new Transition<B>();
    private Transition<C> cTransition = new Transition<C>();

    public A AValue { get => aTransition.value; set => aTransition.value = value; }
    public B BValue { get => bTransition.value; set => bTransition.value = value; }
    public C CValue { get => cTransition.value; set => cTransition.value = value; }

    public bool IsOnTransition => aTransition.IsOnTransition || bTransition.IsOnTransition || cTransition.IsOnTransition;

    #region Public Methods

    public void SetValues(A aValue, B bValue, C cValue)
    {
        AValue = aValue;
        BValue = bValue;
        CValue = cValue;
    }

    public void StartTransition(A aValue, B bValue, C cValue, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, false, false, false, onTransitionEnd);

    public void StartTransition(A aValue, B bValue, C cValue, bool inverseCurve, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, false, inverseCurve, false, onTransitionEnd);

    public void StartTransition(A aValue, B bValue, C cValue, bool inverseCurve, bool scaleDuration, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, false, inverseCurve, scaleDuration, onTransitionEnd);

    public void StartTransitionUnscaled(A aValue, B bValue, C cValue, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, true, false, false, onTransitionEnd);

    public void StartTransitionUnscaled(A aValue, B bValue, C cValue, bool inverseCurve, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, true, inverseCurve, false, onTransitionEnd);

    public void StartTransitionUnscaled(A aValue, B bValue, C cValue, bool inverseCurve, bool scaleDuration, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, true, inverseCurve, scaleDuration, onTransitionEnd);

    /// <summary> Updates transition value when a transition is ocurring. </summary>
    /// <param name="onTransitionUpdate"> Callback to execute when a transition is ocurring. </param>
    public void UpdateTransition(Action<A, B, C> onTransitionUpdate = null)
    {
        aTransition.UpdateTransition();
        bTransition.UpdateTransition();
        cTransition.UpdateTransition();

        isOnTransition = IsOnTransition;
        aValue = AValue;
        bValue = BValue;
        cValue = CValue;

        if (IsOnTransition) onTransitionUpdate?.Invoke(aValue, bValue, cValue);
    }

    /// <summary> End transition and call onTransitionEnd. </summary>
    public void EndTransition()
    {
        aTransition.EndTransition();
        bTransition.EndTransition();
        cTransition.EndTransition();

        isOnTransition = IsOnTransition;
        aValue = AValue;
        bValue = BValue;
        cValue = CValue;
    }

    /// <summary> End transition without calling onTransitionEnd. </summary>
    public void Cancel()
    {
        aTransition.Cancel();
        bTransition.Cancel();
        cTransition.Cancel();

        isOnTransition = IsOnTransition;
        aValue = AValue;
        bValue = BValue;
        cValue = CValue;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void StartTransitionInternal(A aValue, B bValue, C cValue, bool unscaled, bool inverseCurve, bool scaleDuration, Action onTransitionEnd)
    {
        // Set durations
        aTransition.duration = duration;
        bTransition.duration = duration;
        cTransition.duration = duration;

        // Setup curves
        aTransition.curve = aCurve;
        bTransition.curve = bCurve;
        cTransition.curve = cCurve;

        // Launch Transitions
        if (unscaled)
        {
            aTransition.StartTransition(aValue, inverseCurve, scaleDuration, onTransitionEnd);
            bTransition.StartTransition(bValue, inverseCurve, scaleDuration);
            cTransition.StartTransition(cValue, inverseCurve, scaleDuration);
        }
        else
        {
            aTransition.StartTransition(aValue, inverseCurve, scaleDuration, onTransitionEnd);
            bTransition.StartTransition(bValue, inverseCurve, scaleDuration);
            cTransition.StartTransition(cValue, inverseCurve, scaleDuration);
        }

        isOnTransition = IsOnTransition;
    }

    #endregion

}

#endregion

#region MultiTransition x4

[Serializable]
public class MultiTransition<A, B, C, D>
    where A : struct
    where B : struct
    where C : struct
    where D : struct
{
    public float duration = 1;

    [Space]
    public AnimationCurve aCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private A aValue;

    [Space]
    public AnimationCurve bCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private B bValue;

    [Space]
    public AnimationCurve cCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private C cValue;

    [Space]
    public AnimationCurve dCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private D dValue;

    [Space]
    [SerializeField]
    private bool isOnTransition;

    private Transition<A> aTransition = new Transition<A>();
    private Transition<B> bTransition = new Transition<B>();
    private Transition<C> cTransition = new Transition<C>();
    private Transition<D> dTransition = new Transition<D>();

    public A AValue { get => aTransition.value; set => aTransition.value = value; }
    public B BValue { get => bTransition.value; set => bTransition.value = value; }
    public C CValue { get => cTransition.value; set => cTransition.value = value; }
    public D DValue { get => dTransition.value; set => dTransition.value = value; }

    public bool IsOnTransition => aTransition.IsOnTransition || bTransition.IsOnTransition || cTransition.IsOnTransition || dTransition.IsOnTransition;

    #region Public Methods

    public void SetValues(A aValue, B bValue, C cValue, D dValue)
    {
        AValue = aValue;
        BValue = bValue;
        CValue = cValue;
        DValue = dValue;
    }

    public void StartTransition(A aValue, B bValue, C cValue, D dValue, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, dValue, false, false, false, onTransitionEnd);

    public void StartTransition(A aValue, B bValue, C cValue, D dValue, bool inverseCurve, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, dValue, false, inverseCurve, false, onTransitionEnd);

    public void StartTransition(A aValue, B bValue, C cValue, D dValue, bool inverseCurve, bool scaleDuration, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, dValue, false, inverseCurve, scaleDuration, onTransitionEnd);

    public void StartTransitionUnscaled(A aValue, B bValue, C cValue, D dValue, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, dValue, true, false, false, onTransitionEnd);

    public void StartTransitionUnscaled(A aValue, B bValue, C cValue, D dValue, bool inverseCurve, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, dValue, true, inverseCurve, false, onTransitionEnd);

    public void StartTransitionUnscaled(A aValue, B bValue, C cValue, D dValue, bool inverseCurve, bool scaleDuration, Action onTransitionEnd = null) => StartTransitionInternal(aValue, bValue, cValue, dValue, true, inverseCurve, scaleDuration, onTransitionEnd);

    /// <summary> Updates transition value when a transition is ocurring. </summary>
    /// <param name="onTransitionUpdate"> Callback to execute when a transition is ocurring. </param>
    public void UpdateTransition(Action<A, B, C, D> onTransitionUpdate = null)
    {
        aTransition.UpdateTransition();
        bTransition.UpdateTransition();
        cTransition.UpdateTransition();
        dTransition.UpdateTransition();

        isOnTransition = IsOnTransition;
        aValue = AValue;
        bValue = BValue;
        cValue = CValue;
        dValue = DValue;

        if (IsOnTransition) onTransitionUpdate?.Invoke(aValue, bValue, cValue, dValue);
    }

    /// <summary> End transition and call onTransitionEnd. </summary>
    public void EndTransition()
    {
        aTransition.EndTransition();
        bTransition.EndTransition();
        cTransition.EndTransition();
        dTransition.EndTransition();

        isOnTransition = IsOnTransition;
        aValue = AValue;
        bValue = BValue;
        cValue = CValue;
        dValue = DValue;
    }

    /// <summary> End transition without calling onTransitionEnd. </summary>
    public void Cancel()
    {
        aTransition.Cancel();
        bTransition.Cancel();
        cTransition.Cancel();
        dTransition.Cancel();

        isOnTransition = IsOnTransition;
        aValue = AValue;
        bValue = BValue;
        cValue = CValue;
        dValue = DValue;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void StartTransitionInternal(A aValue, B bValue, C cValue, D dValue, bool unscaled, bool inverseCurve, bool scaleDuration, Action onTransitionEnd)
    {
        // Set durations
        aTransition.duration = duration;
        bTransition.duration = duration;
        cTransition.duration = duration;
        dTransition.duration = duration;

        // Setup curves
        aTransition.curve = aCurve;
        bTransition.curve = bCurve;
        cTransition.curve = cCurve;
        dTransition.curve = dCurve;

        // Launch Transitions
        if (unscaled)
        {
            aTransition.StartTransition(aValue, inverseCurve, scaleDuration, onTransitionEnd);
            bTransition.StartTransition(bValue, inverseCurve, scaleDuration);
            cTransition.StartTransition(cValue, inverseCurve, scaleDuration);
            dTransition.StartTransition(dValue, inverseCurve, scaleDuration);
        }
        else
        {
            aTransition.StartTransition(aValue, inverseCurve, scaleDuration, onTransitionEnd);
            bTransition.StartTransition(bValue, inverseCurve, scaleDuration);
            cTransition.StartTransition(cValue, inverseCurve, scaleDuration);
            dTransition.StartTransition(dValue, inverseCurve, scaleDuration);
        }

        isOnTransition = IsOnTransition;
    }

    #endregion

}

#endregion