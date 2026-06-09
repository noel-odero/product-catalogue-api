using ProductCatalogue.DTOs.Auth;

namespace ProductCatalogue.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}