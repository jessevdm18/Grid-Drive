/// <summary>
/// Federated / local identity providers.
/// Ownership of Daily data is always Firebase UID — never these provider IDs.
/// </summary>
public enum AccountProvider
{
    Anonymous = 0,
    GooglePlay = 1,
    Apple = 2
}
