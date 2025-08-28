// 파일 경로: Scripts/4_Utilities/Log.cs

using UnityEngine;

public static class Log
{
    public static void I(string message)
    {
        if (DebugManager.Instance != null && DebugManager.Instance.LogLevel >= LogLevel.Normal)
            Debug.Log($"<color=#8ec07c>[정보]</color> {message}");
    }

    public static void V(string message) // Verbose(상세) 레벨 추가
    {
        if (DebugManager.Instance != null && DebugManager.Instance.LogLevel >= LogLevel.Verbose)
            Debug.Log($"<color=#b8bb26>[상세]</color> {message}");
    }

    public static void W(string message)
    {
        if (DebugManager.Instance != null && DebugManager.Instance.LogLevel >= LogLevel.Normal)
            Debug.LogWarning($"<color=#fabd2f>[경고]</color> {message}");
    }

    public static void E(string message)
    {
        if (DebugManager.Instance != null && DebugManager.Instance.LogLevel >= LogLevel.ErrorsOnly)
            Debug.LogError($"<color=#fb4934>[오류]</color> {message}");
    }
}