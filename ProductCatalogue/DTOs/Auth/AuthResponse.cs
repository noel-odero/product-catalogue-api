namespace ProductCatalogue.DTOs.Auth;

public class AuthResponse
{
    public required string Token {get; init;}
    public DateTimeOffset ExpiresAt {get; init;}
    public required UserInfo User {get; init;}
}

public class UserInfo
{
    public Guid Id {get; init;}
    public required string Email {get; init;}
    public required string FirstName {get; init;}
    public required string LastName {get; init;}
}