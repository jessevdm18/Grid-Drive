using System.Threading.Tasks;

/// <summary>
/// External account provider boundary. Identity service links credentials to Firebase;
/// Daily backend never sees provider-specific IDs.
/// </summary>
public interface IExternalAccountProvider
{
    AccountProvider Provider { get; }

    /// <summary>False in Editor / missing SDK / unsupported platform.</summary>
    bool IsAvailable { get; }

    Task<ExternalAccountAuthResult> AuthenticateForLinkAsync();
}

/// <summary>Result of provider-side authentication before Firebase linking.</summary>
public readonly struct ExternalAccountAuthResult
{
    public AccountLinkResult Result { get; }
    public ExternalAccountCredential Credential { get; }
    public string Message { get; }

    public ExternalAccountAuthResult(
        AccountLinkResult result,
        ExternalAccountCredential credential = null,
        string message = null)
    {
        Result = result;
        Credential = credential;
        Message = message ?? string.Empty;
    }

    public static ExternalAccountAuthResult Ok(ExternalAccountCredential credential)
    {
        return new ExternalAccountAuthResult(AccountLinkResult.Success, credential);
    }

    public static ExternalAccountAuthResult Fail(AccountLinkResult result, string message = null)
    {
        return new ExternalAccountAuthResult(result, null, message);
    }
}
