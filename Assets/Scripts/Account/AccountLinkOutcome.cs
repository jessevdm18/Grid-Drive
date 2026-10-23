/// <summary>Result envelope for account link operations (UI must not parse raw strings).</summary>
public readonly struct AccountLinkOutcome
{
    public AccountLinkResult Result { get; }
    public string Message { get; }
    public string ProviderKey { get; }

    public bool IsSuccess =>
        Result == AccountLinkResult.Success || Result == AccountLinkResult.AlreadyLinked;

    public AccountLinkOutcome(AccountLinkResult result, string message, string providerKey)
    {
        Result = result;
        Message = message ?? string.Empty;
        ProviderKey = providerKey ?? string.Empty;
    }

    public static AccountLinkOutcome From(AccountLinkResult result, string providerKey, string message = null)
    {
        return new AccountLinkOutcome(result, message ?? DefaultMessage(result), providerKey);
    }

    public static string DefaultMessage(AccountLinkResult result)
    {
        switch (result)
        {
            case AccountLinkResult.Success:
                return "Google Play account linked.";
            case AccountLinkResult.AlreadyLinked:
                return "Google Play account linked.";
            case AccountLinkResult.AlreadyLinkedToAnotherAccount:
                return "This Google Play account is already linked to another Rush Out account.";
            case AccountLinkResult.Cancelled:
                return "Google Play Games sign-in was cancelled.";
            case AccountLinkResult.ProviderUnavailable:
                return "Google Play Games is unavailable on this device.";
            case AccountLinkResult.ProviderBackendUnavailable:
                return "Google Play Games sign-in is temporarily unavailable. Please try again later.";
            case AccountLinkResult.NetworkError:
                return "Couldn't connect to Google Play Games. Check your internet connection and try again.";
            case AccountLinkResult.AuthenticationError:
                return "Google Play Games sign-in failed. Make sure you're signed in to Google Play Games and try again.";
            case AccountLinkResult.IdentityContinuityError:
                return "Account link failed. Your progress was not moved.";
            default:
                return "Could not connect. Try again.";
        }
    }
}
