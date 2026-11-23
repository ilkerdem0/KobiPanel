using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KobiPanel.Business.Services;

namespace KobiPanel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    public record RegisterRequest(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string? PhoneNumber
    );

    public record LoginRequest(string Email, string Password);
    public record RefreshRequest(string RefreshToken);

    /// <summary>
    /// Yeni kullanıcı kaydı
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(
            request.FirstName, request.LastName,
            request.Email, request.Password, request.PhoneNumber);
        return Ok(result);
    }

    /// <summary>
    /// Kullanıcı girişi
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);
        return Ok(result);
    }

    /// <summary>
    /// Token yenileme
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(result);
    }

    /// <summary>
    /// Çıkış yap
    /// </summary>
    // Refresh token'a sahip olmak iptal için yeterli; access token süresi dolmuş olsa da çıkış yapılabilmeli
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await _authService.RevokeTokenAsync(request.RefreshToken);
        return Ok(new { message = "Çıkış yapıldı" });
    }
}