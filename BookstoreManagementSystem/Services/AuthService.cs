using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookstoreManagementSystem.Data;
using BookstoreManagementSystem.DTOs.Auth;
using BookstoreManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BookstoreManagementSystem.Services;

public class AuthService : IAuthService
{
    private readonly BookstoreDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(BookstoreDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
        {
            _logger.LogWarning("Registration failed: Username {Username} already exists", registerDto.Username);
            return null;
        }

        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists", registerDto.Email);
            return null;
        }

        var readRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Read");
        if (readRole == null)
        {
            _logger.LogError("Registration failed: Read role not found in database");
            throw new InvalidOperationException("Read role not found in database");
        }

        var passwordHash = HashPassword(registerDto.Password);

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            RoleId = readRole.Id
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _context.Entry(user).Reference(u => u.Role).LoadAsync();

        var token = GenerateJwtToken(user.Id, user.Username, user.Role.Name);

        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.Name
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User {Username} not found or inactive", loginDto.Username);
            return null;
        }

        if (!VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for user {Username}", loginDto.Username);
            return null;
        }

        var token = GenerateJwtToken(user.Id, user.Username, user.Role.Name);

        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.Name
        };
    }

    public string GenerateJwtToken(int userId, string username, string role)
    {
        _logger.LogDebug("Generating JWT token for UserId: {UserId}, Username: {Username}, Role: {Role}",
            userId, username, role);

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["ExpirationHours"] ?? "24")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == passwordHash;
    }
}
