using System.Threading.Tasks;

/// <summary>Stub Apple provider — Phase 4.2A does not implement Sign in with Apple.</summary>
public sealed class UnavailableAppleAccountProvider : IAppleAccountProvider
{
    public AccountProvider Provider => AccountProvider.Apple;

    public bool IsAvailable => false;

    public Task<ExternalAccountAuthResult> AuthenticateForLinkAsync()
    {
        return Task.FromResult(
            ExternalAccountAuthResult.Fail(
                AccountLinkResult.ProviderUnavailable,
                "Sign in with Apple is not implemented yet."));
    }
}
