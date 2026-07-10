using BookstoreManagementSystem.DTOs.Auth;

namespace BookstoreManagementSystem.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    string GenerateJwtToken(int userId, string username, string role);
}
