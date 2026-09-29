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
                return "Google Play linked.";
            case AccountLinkResult.AlreadyLinked:
                return "Google Play linked.";
            case AccountLinkResult.AlreadyLinkedToAnotherAccount:
                return "This Google Play account is already linked to another Rush Out account.";
            case AccountLinkResult.Cancelled:
                return "Linking cancelled.";
            case AccountLinkResult.ProviderUnavailable:
                return "Google Play is not available on this device.";
            case AccountLinkResult.NetworkError:
                return "Could not connect. Try again.";
            case AccountLinkResult.AuthenticationError:
                return "Could not connect. Try again.";
            case AccountLinkResult.IdentityContinuityError:
                return "Account link failed. Your progress was not moved.";
            default:
                return "Could not connect. Try again.";
        }
    }
}
