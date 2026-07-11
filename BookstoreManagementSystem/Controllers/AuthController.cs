using BookstoreManagementSystem.DTOs.Auth;
using BookstoreManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid registration attempt from IP: {ClientIP}, Username: {Username}",
                clientIp, registerDto.Username);
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(registerDto);

        if (result == null)
        {
            _logger.LogWarning("Registration failed for username: {Username} - Username or email already exists",
                registerDto.Username);
            return BadRequest(new { message = "Username or email already exists" });
        }

        _logger.LogInformation("User registered successfully: {Username} from IP: {ClientIP}",
            registerDto.Username, clientIp);
        return Ok(result);
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid login attempt from IP: {ClientIP}, Username: {Username}",
                clientIp, loginDto.Username);
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(loginDto);

        if (result == null)
        {
            _logger.LogWarning("Failed login attempt for username: {Username} from IP: {ClientIP}",
                loginDto.Username, clientIp);
            return Unauthorized(new { message = "Invalid username or password" });
        }

        _logger.LogInformation("Successful login for username: {Username} from IP: {ClientIP}",
            loginDto.Username, clientIp);
        return Ok(result);
    }
}
