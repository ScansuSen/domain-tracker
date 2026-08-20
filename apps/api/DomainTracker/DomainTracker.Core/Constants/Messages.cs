namespace DomainTracker.Core.Constants;

public static class Messages
{
    public const string UserRegisteredSuccessfully = "User registered successfully.";

    public const string LoginSuccessful = "Login successful.";

    public const string InvalidCredentials = "Invalid username or password.";

    public static string UsernameAlreadyTaken(string username) => $"Username '{username}' is already taken.";

    public const string DomainAddedToFavorites = "Domain added to favorites.";

    public const string FavoriteRemoved = "Favorite removed.";

    public const string FavoriteRefreshed = "Favorite refreshed.";

    public static string DomainAlreadyInFavorites(string domainName) => $"'{domainName}' is already in your favorites.";

    public static string FavoriteNotFound(int favoriteId) => $"Favorite with id '{favoriteId}' was not found.";

    public static string DomainNotPersistedAfterCheck(string domainName) =>
        $"Domain '{domainName}' was not persisted by the availability check.";

    public const string RdapServiceUnreachable = "Unable to reach the domain availability service. Please try again later.";

    public const string UnexpectedError = "An unexpected error occurred. Please try again later.";

    public const string AuthenticationRequired = "Authentication required, or the token is missing/invalid/expired.";

    public const string JwtKeyMissing = "Missing required 'Jwt:Key' configuration value.";

    public const string UsernameRequired = "Username is required.";

    public const string UsernameLengthInvalid = "Username must be between 3 and 100 characters.";

    public const string UsernameFormatInvalid = "Username may only contain letters, digits, '.', '_' and '-'.";

    public const string PasswordRequired = "Password is required.";

    public const string PasswordTooShort = "Password must be at least 6 characters long.";

    public const string PasswordTooLong = "Password must be at most 100 characters long.";

    public const string DomainNameRequired = "Domain name is required.";

    public static string DomainNameTooLong(int maxLength) => $"Domain name must be at most {maxLength} characters.";

    public const string DomainNameFormatInvalid = "'{PropertyValue}' is not a valid domain name.";
}
