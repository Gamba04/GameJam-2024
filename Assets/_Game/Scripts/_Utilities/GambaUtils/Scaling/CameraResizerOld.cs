using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GambaUtils.Resizers.Deprecated
{
    [ExecuteAlways]
    public class CameraResizerOld : MonoBehaviour
    {
        private enum TangentType
        {
            Linear,
            Smooth
        }

        [Serializable]
        private class AspectRatio
        {
            [SerializeField, HideInInspector] private string name;

            public Vector2 aspect;
            public float size;

            [ReadOnly, SerializeField]
            private float ratio;

            public float GetRatio() => aspect.x / aspect.y;

            public AspectRatio()
            {
                aspect = new Vector2(16, 9);
                size = 5;
            }

            public AspectRatio(Vector2 aspect, float size)
            {
                this.aspect = aspect;
                this.size = size;
            }

            public void SetName()
            {
                name = $"{aspect.x}:{aspect.y} - {size}";

                ratio = GetRatio();
            }
        }

        public bool webGL;

        [Space]
        [SerializeField]
        private new Camera camera;

        [Space]
        [SerializeField]
        private List<AspectRatio> aspectRatios = new List<AspectRatio>();

        [Header("Curve")]
        [SerializeField]
        private TangentType tangents;
        [SerializeField]
        private AnimationCurve curve;

        [Space]
        [ReadOnly, SerializeField]
        private float currentRatio;

        #region Start

        private void Start()
        {
            if (Application.isPlaying)
            {
                if (!webGL)
                {
                    ResizeCamera();
                }
                else
                {
                    StartCoroutine(DelayedStart());
                }
            }
            else
            {
                // Default settings
                if (aspectRatios.Count == 0)
                {
                    aspectRatios.Add(new AspectRatio(new Vector2(3, 4), 4));
                    aspectRatios.Add(new AspectRatio(new Vector2(9, 16), 5));
                    aspectRatios.Add(new AspectRatio(new Vector2(9, 21), 7));
                }
            }
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();

            ResizeCamera();
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------------------------------

        #region Other Methods

        private void GenerateCurve()
        {
            curve = new AnimationCurve();

            // Generate Keyframes
            for (int i = 0; i < aspectRatios.Count; i++)
            {
                curve.AddKey(new Keyframe(aspectRatios[i].GetRatio(), aspectRatios[i].size));
            }

            switch (tangents)
            {
                case TangentType.Linear:
                    LinearTangents(ref curve);
                    break;

                case TangentType.Smooth:
                    SmoothTangents(ref curve);
                    break;
            }
        }

        private void LinearTangents(ref AnimationCurve curve)
        {

#if UNITY_EDITOR

            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }

#endif

        }

        private void SmoothTangents(ref AnimationCurve curve)
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

        private void ResizeCamera()
        {
            if (camera != null && curve.keys.Length > 0)
            {
                currentRatio = camera.aspect;
                camera.orthographicSize = curve.Evaluate(currentRatio);
            }
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------------------------------

        #region Editor

#if UNITY_EDITOR

        private void Update()
        {
            if (!Application.isPlaying)
            {
                for (int i = 0; i < aspectRatios.Count; i++)
                {
                    aspectRatios[i].SetName();
                }

                if (camera == null)
                {
                    camera = GetComponent<Camera>();
                }

                GenerateCurve();
                ResizeCamera();
            }
        }

#endif

        #endregion

    }
}