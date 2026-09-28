using System;

/// <summary>Result envelope for authority operations.</summary>
[Serializable]
public struct DailyChallengeAuthorityResult
{
    public bool Success;
    public string ErrorCode;
    public string ErrorMessage;
    public DailyChallengeDescriptor Challenge;
    public DailyChallengeAttemptRecord Attempt;

    public static DailyChallengeAuthorityResult Ok(
        DailyChallengeDescriptor challenge,
        DailyChallengeAttemptRecord attempt)
    {
        return new DailyChallengeAuthorityResult
        {
            Success = true,
            Challenge = challenge,
            Attempt = attempt
        };
    }

    public static DailyChallengeAuthorityResult Fail(
        string code,
        string message,
        DailyChallengeDescriptor challenge = default,
        DailyChallengeAttemptRecord attempt = default)
    {
        return new DailyChallengeAuthorityResult
        {
            Success = false,
            ErrorCode = code ?? "error",
            ErrorMessage = message ?? string.Empty,
            Challenge = challenge,
            Attempt = attempt
        };
    }
}
