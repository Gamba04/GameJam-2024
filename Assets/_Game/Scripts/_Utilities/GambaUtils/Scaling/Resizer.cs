using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GambaUtils.Resizers
{
    [ExecuteAlways]
    public abstract class Resizer : MonoBehaviour
    {

        #region Custom Data

        private enum TangentType
        {
            Linear,
            Smooth
        }

        [Serializable]
        private class AspectRatioData
        {
            [SerializeField, HideInInspector] private string name;

            public Vector2 aspect;
            public float size;

            [ReadOnly, SerializeField]
            private float ratio;

            public float Ratio => aspect.x / aspect.y;

            public AspectRatioData(Vector2 aspect, float size)
            {
                this.aspect = aspect;
                this.size = size;
            }

            public void EditorUpdate()
            {
                aspect.x = Mathf.Max(aspect.x, 1);
                aspect.y = Mathf.Max(aspect.y, 1);

                name = $"{aspect.x}:{aspect.y}";

                ratio = Ratio;
            }
        }

        #endregion

        [Space]
        [SerializeField]
        private bool updateInEditor = true;

        [Space]
        [SerializeField]
        private List<AspectRatioData> aspectRatios = new List<AspectRatioData>();
        [ReadOnly, SerializeField]
        private float currentRatio;

        [Header("Curve")]
        [SerializeField]
        private TangentType tangents;
        [SerializeField]
        private AnimationCurve curve;

        private float CurrentRatio => (float)Screen.width / Screen.height;

        #region Start

        private void Start()
        {
            if (Application.isPlaying)
            {
                RuntimeStart();
            }
            else
            {
                EditorStart();
            }
        }

        private void RuntimeStart()
        {
#if UNITY_WEBGL
                Delay(Resize);

                static System.Collections.IEnumerator Delay(Action action)
                {
                    yield return null;

                    action?.Invoke();
                }
#else
            Resize();
#endif
        }

        private void EditorStart()
        {
            if (aspectRatios.Count > 0) return;

            aspectRatios.Add(new AspectRatioData(new Vector2( 4, 3), 1));
            aspectRatios.Add(new AspectRatioData(new Vector2(16, 9), 1));
            aspectRatios.Add(new AspectRatioData(new Vector2(21, 9), 1));
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------------------------------

        #region Resize

        private void Resize()
        {
            if (curve.keys.Length == 0) return;

            float size = curve.Evaluate(CurrentRatio);
            ApplySize(size);
        }

        protected abstract void ApplySize(float size);

        #endregion

        // ----------------------------------------------------------------------------------------------------------------------------

        #region Editor

#if UNITY_EDITOR

        #region Update

        private void Update()
        {
            if (!Application.isPlaying)
            {
                EditorUpdate();
            }
        }

        protected virtual void EditorUpdate()
        {
            currentRatio = CurrentRatio;
            aspectRatios.ForEach(aspect => aspect.EditorUpdate());

            GenerateCurve();
            if (updateInEditor) Resize();
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------------------------------

        #region Curve

        private void GenerateCurve()
        {
            curve = new AnimationCurve();

            // Generate Keyframes
            aspectRatios.ForEach(aspect => curve.AddKey(new Keyframe(aspect.Ratio, aspect.size)));

            // Set tangents
            switch (tangents)
            {
                case TangentType.Linear:
                    SetTangentsLinear(ref curve);
                    break;

                case TangentType.Smooth:
                    SetTangentsSmooth(ref curve);
                    break;
            }
        }

        private static void SetTangentsLinear(ref AnimationCurve curve)
        {
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }
        }

        private static void SetTangentsSmooth(ref AnimationCurve curve)
        {
            for (int i = 0; i < curve.length; i++)
            {
                int prev = i - 1;
                int next = i + 1;

                Keyframe prevKey = curve.keys[prev < 0 ? 0 : prev];
                Keyframe nextKey = curve.keys[next > curve.length - 1 ? curve.length - 1 : next];
                Keyframe currKey = curve.keys[i];

                float inTangent = (currKey.value - prevKey.value) / (currKey.time - prevKey.time);
                float outTangent = (nextKey.value - currKey.value) / (nextKey.time - currKey.time);

                if (prev < 0) inTangent = outTangent;
                if (next > curve.length - 1) outTangent = inTangent;

                float average = (outTangent + inTangent) / 2f;

                currKey.inTangent = average;
                currKey.outTangent = average;

                curve.MoveKey(i, currKey);
            }
        }

        #endregion

#endif

        #endregion

    }
}