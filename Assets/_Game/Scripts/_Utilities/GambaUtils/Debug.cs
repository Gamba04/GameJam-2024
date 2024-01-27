using System;
using UnityEngine;

public static class Debug
{
    private static bool Debugs => GplayManager.Debugs;

    #region Public Methods

    #region Log

    public static void Log(object message) => FilterDebugs(() => UnityEngine.Debug.Log(ProcessMessage(message)));

    public static void Log(object message, Color color = default, FontStyle fontStyle = default) => FilterDebugs(() => UnityEngine.Debug.Log(ProcessMessage(message, color, fontStyle)));

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Log Warning

    public static void LogWarning(object message) => FilterDebugs(() => UnityEngine.Debug.LogWarning(ProcessMessage(message)));

    public static void LogWarning(object message, Color color = default, FontStyle fontStyle = default) => FilterDebugs(() => UnityEngine.Debug.LogWarning(ProcessMessage(message, color, fontStyle)));

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Log Error

    public static void LogError(object message) => FilterDebugs(() => UnityEngine.Debug.LogError(ProcessMessage(message)));

    public static void LogError(object message, Color color = default, FontStyle fontStyle = default) => FilterDebugs(() => UnityEngine.Debug.LogError(ProcessMessage(message, color, fontStyle)));

    #endregion

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Processing

    #region Main

    private static void FilterDebugs(Action method)
    {

#if UNITY_EDITOR

        if (Debugs) method?.Invoke();

#endif

    }

    private static string ProcessMessage(object message, Color color, FontStyle fontStyle)
    {
        string text = message.ToString();

        ApplyColor(ref text, color);
        ApplyStyle(ref text, fontStyle);

        return ProcessMessage(text);
    }

    private static string ProcessMessage(object message) => $"{message}\n";

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Color

    private static void ApplyColor(ref string text, Color color)
    {
        if (color.a == 0) return;

        string hexColor = ColorUtility.ToHtmlStringRGBA(color);

        text = $"<color=#{hexColor}>{text}</color>";
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Font Style

    private static void ApplyStyle(ref string text, FontStyle fontStyle)
    {
        text = fontStyle switch
        {
            FontStyle.Bold => ApplyBold(text),
            FontStyle.Italic => ApplyItalic(text),
            FontStyle.BoldAndItalic => ApplyBold(ApplyItalic(text)),
            _ => text
        };
    }

    private static string ApplyBold(string text) => $"<b>{text}</b>";

    private static string ApplyItalic(string text) => $"<i>{text}</i>";

    #endregion

    #endregion

}