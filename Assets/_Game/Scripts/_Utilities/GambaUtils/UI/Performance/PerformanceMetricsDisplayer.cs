using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerformanceMetricsDisplayer : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private PerformanceMetrics performanceMetrics;
    [SerializeField]
    private Text text;

    [Header("Settings")]
    [SerializeField]
    [Range(0, 1)]
    private float refreshDelay = 1;

    #region Start

    private void Start()
    {
        InitEvents();
        StartRefreshLoop();
    }

    private void InitEvents()
    {
        performanceMetrics.onSetOptions += options => Refresh();
    }

    private void StartRefreshLoop()
    {
        StartCoroutine(RefreshLoop());
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Refresh

    private IEnumerator RefreshLoop()
    {
        while (true)
        {
            Refresh();

            yield return refreshDelay > 0 ? new WaitForSeconds(refreshDelay) : null;
        }
    }

    private void Refresh()
    {
        if (text == null) return;

        text.text = GetText();
    }

    private string GetText()
    {
        string text = "";

        List<Metric> metrics = performanceMetrics.metrics.ActiveMetrics;
        bool showLabels = performanceMetrics.GetOptions().showLabels;

        foreach (Metric metric in metrics)
        {
            string name = showLabels ? $"{metric.Name}: " : "";

            text += $"{name}{metric.GetValue()}{metric.Unit}\n";
        }

        return text;
    }

    #endregion

}