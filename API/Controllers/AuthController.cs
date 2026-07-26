using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/Auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var token = await _authService.Login(dto);
            if (token == null)
                return Unauthorized(new { Message = "Invalid credentials" });

            return Ok(token);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { Message = "Invalid credentials" });
        }
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var token = await _authService.RefreshToken(request);
            if (token == null)
                return Unauthorized(new { Message = "Invalid refresh token" });

            return Ok(token);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { Message = "Invalid refresh token" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { Message = "Invalid or missing user identifier" });

        try
        {
            var result = await _authService.Logout(userId, request.RefreshToken);
            if (result == true)
                return Ok(new { Message = "Logged out successfully" });

            return BadRequest(new { Message = "Logout failed" });
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { Message = "Logout is not yet implemented" });
        }
    }
}
