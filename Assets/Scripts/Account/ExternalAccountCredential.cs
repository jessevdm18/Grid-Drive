/// <summary>
/// Provider credential payload for Firebase account linking.
/// Do not log or persist auth codes / tokens from this object.
/// </summary>
public sealed class ExternalAccountCredential
{
    public AccountProvider Provider { get; set; }

    /// <summary>Firebase Identity Toolkit providerId, e.g. playgames.google.com</summary>
    public string FirebaseProviderId { get; set; }

    /// <summary>
    /// URL-encoded postBody parameters for accounts:signInWithIdp (without wrapping).
    /// Example Play Games: authCode=CODE plus providerId=playgames.google.com
    /// </summary>
    public string IdpPostBody { get; set; }

    /// <summary>Optional presentation-only display name from the provider.</summary>
    public string DisplayNameHint { get; set; }
}
