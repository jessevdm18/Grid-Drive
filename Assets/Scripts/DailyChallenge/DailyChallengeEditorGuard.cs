using System;
using UnityEngine;

/// <summary>
/// Stops Editor Play Mode from calling a real Online Daily attempt.
/// Player builds do not take these branches.
/// </summary>
public static class DailyChallengeEditorGuard
{
    public const string Message =
        "Real Daily Challenges are disabled in the Unity Editor.\nUse Rush Out > Daily > Play Selected as Daily to test safely.";

#if UNITY_EDITOR
    private static int readOnlyPermitDepth;

    public static bool BlocksRealAttempts => true;

    /// <summary>
    /// Editor menu diagnostics may read getDailyChallenge. Attempt mutations stay blocked.
    /// </summary>
    public static IDisposable PermitReadOnlyDiagnostic()
    {
        readOnlyPermitDepth++;
        return new ReadOnlyPermit();
    }

    public static bool ShouldBlockCallable(string operation)
    {
        if (IsReadOnlyDiagnostic(operation) && readOnlyPermitDepth > 0)
        {
            return false;
        }

        Debug.LogWarning(
            "[DailyEditorGuard] Blocked real Daily backend call: " + operation);
        return true;
    }

    private static bool IsReadOnlyDiagnostic(string operation)
    {
        return string.Equals(operation, "getDailyChallenge", StringComparison.Ordinal);
    }

    private sealed class ReadOnlyPermit : IDisposable
    {
        public void Dispose()
        {
            if (readOnlyPermitDepth > 0)
            {
                readOnlyPermitDepth--;
            }
        }
    }
#else
    public static bool BlocksRealAttempts => false;
#endif
}
