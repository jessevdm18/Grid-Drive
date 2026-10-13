/// <summary>Structured outcome for optional provider link attempts.</summary>
public enum AccountLinkResult
{
    Success = 0,
    AlreadyLinked = 1,
    AlreadyLinkedToAnotherAccount = 2,
    Cancelled = 3,
    ProviderUnavailable = 4,
    NetworkError = 5,
    AuthenticationError = 6,
    IdentityContinuityError = 7,
    UnknownError = 8,
    /// <summary>
    /// Kept so older result values stay stable. New Play Games link failures use
    /// UnknownError / AuthenticationError instead of treating Firebase rejection as
    /// Play Games being unavailable.
    /// </summary>
    ProviderBackendUnavailable = 9
}
