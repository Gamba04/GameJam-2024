using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#region Custom Data

[Serializable]
    public struct PerformanceOptions
    {
        [Serializable]
        public struct Advanced
        {
            public bool enabled;

            [Space]
            public bool VSync;
            public bool WaitForGPU;
            public bool Render;
            public bool Update;
        }

        public bool showLabels;

        public bool FPS;

        public Advanced advanced;
    }

    public class MetricsProcessor
    {
        public readonly FPS fps = new FPS();
        public readonly VSyncPercentage vsync = new VSyncPercentage();
        public readonly GPUPercentage waitForGPU = new GPUPercentage();
        public readonly RenderPercentage render = new RenderPercentage();
        public readonly UpdatePercentage update = new UpdatePercentage();

        public List<Metric> ActiveMetrics => Metrics.FindAll(metric => metric.active);

        public List<Metric> Metrics => new List<Metric>()
        {
            fps,
            vsync,
            waitForGPU,
            render,
            update,
        };

        public void OnEnable() => Metrics.ForEach(metric => metric.OnEnable());

        public void OnDisable() => Metrics.ForEach(metric => metric.OnDisable());

        public void SetOptions(PerformanceOptions options)
        {
            fps.active = options.FPS;

            bool advanced = options.advanced.enabled;

            vsync.active = advanced && options.advanced.VSync;
            waitForGPU.active = advanced && options.advanced.WaitForGPU;
            render.active = advanced && options.advanced.Render;
            update.active = advanced && options.advanced.Update;
        }
    }

    #region Metrics

    public abstract class Metric
    {
        public bool active;

        public abstract string Name { get; }

        public abstract string Unit { get; }

        public abstract float GetValue();

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        protected float RoundToDecimalPlaces(float value, uint decimalPoints)
        {
            float exp = Mathf.Pow(10, decimalPoints);

            return Mathf.Round(value * exp) / exp;
        }
    }

    public class FPS : Metric
    {
        public override string Name => "FPS";

        public override string Unit => default;

        public override float GetValue() => Mathf.Round(1 / Time.unscaledDeltaTime);
    }

    #region ProfileRecorders

    public abstract class ProfileRecorderMetric : Metric
    {
        protected virtual string StatName { get; }

        protected virtual uint DecimalPlaces => 2;

        protected ProfilerRecorder recorder;

        public override void OnEnable() => recorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, StatName);

        public override void OnDisable() => recorder.Dispose();
    }

    public abstract class TimeRecorder : ProfileRecorderMetric
    {
        public sealed override string Unit => "ms";

        public override float GetValue() => RoundToDecimalPlaces(recorder.LastValue / 1e6f, DecimalPlaces);
    }

    public abstract class PercentageRecorder : ProfileRecorderMetric
    {
        public sealed override string Unit => "%";

        public override float GetValue() => RoundToDecimalPlaces(100f * (recorder.LastValue / 1e9f) / Time.unscaledDeltaTime, DecimalPlaces);
    }

    public class VSyncPercentage : PercentageRecorder
    {
        public override string Name => "VSync Bottleneck";

        protected override string StatName => "WaitForTargetFPS";
    }

    public class GPUPercentage : PercentageRecorder
    {
        public override string Name => "GPU Bottleneck";

        protected override string StatName => "Gfx.WaitForPresentOnGfxThread";
    }

    public class RenderPercentage : PercentageRecorder
    {
        public override string Name => "Render";

        protected override string StatName => "Camera.Render";
    }

    public class UpdatePercentage : PercentageRecorder
    {
        public override string Name => "Update";

        protected override string StatName => "BehaviourUpdate";
    }

#endregion

#endregion

#endregion

public class PerformanceMetrics : MonoBehaviour
{
    [SerializeField]
    private PerformanceOptions options;

    public readonly MetricsProcessor metrics = new MetricsProcessor();

    public event Action<PerformanceOptions> onSetOptions;

    private static PerformanceOptions? storedOptions;

    #region Awake

    private void Awake()
    {
        ResfreshOptions();
    }

    private void ResfreshOptions()
    {
        SetOptions(GetOptions());
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public PerformanceOptions GetOptions()
    {
        if (storedOptions.HasValue) options = storedOptions.Value;

        return options;
    }

    public void SetOptions(PerformanceOptions options)
    {
        this.options = options;
        storedOptions = options;

        metrics.SetOptions(options);

        onSetOptions?.Invoke(options);
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void OnEnable() => metrics.OnEnable();

    private void OnDisable() => metrics.OnDisable();

    #endregion

}