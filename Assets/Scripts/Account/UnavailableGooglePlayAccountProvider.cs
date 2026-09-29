using System.Threading.Tasks;

/// <summary>
/// Fallback Google Play adapter for Editor, iOS, and unsupported platforms.
/// Android player builds register <see cref="GooglePlayGamesAccountProvider"/> instead
/// via <see cref="GooglePlayAccountProviderRegistration"/> (no startup auth).
/// </summary>
public sealed class UnavailableGooglePlayAccountProvider : IGooglePlayAccountProvider
{
    public AccountProvider Provider => AccountProvider.GooglePlay;

    public bool IsAvailable => false;

    public Task<ExternalAccountAuthResult> AuthenticateForLinkAsync()
    {
        return Task.FromResult(
            ExternalAccountAuthResult.Fail(
                AccountLinkResult.ProviderUnavailable,
                "Google Play linking is not available in the Editor or on this platform."));
    }
}
