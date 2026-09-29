/// <summary>Presentation/domain state for optional account linking.</summary>
public enum AccountLinkState
{
    Unknown = 0,
    GuestAnonymous = 1,
    LinkedGooglePlay = 2,
    LinkedApple = 3,
    LinkedMultiple = 4,
    Linking = 5,
    Error = 6
}
