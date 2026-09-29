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
    UnknownError = 8
}
