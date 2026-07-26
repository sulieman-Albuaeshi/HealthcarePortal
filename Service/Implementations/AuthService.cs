using Service.DTOs;
using Service.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

using Repository.Interfaces;

namespace Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userService, IRefreshTokenRepository refreshTokenService, IConfiguration config)
    {
        _userRepository = userService;
        _refreshTokenRepository = refreshTokenService;
        _config = config;
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);;
    }

    public async Task<TokenDto?> Login(LoginDto userLoginDto)
    {
        var user = await _userRepository.GetByEmailAsync(userLoginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");
            
        var AccessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        var refreshTokenExpiryDays = Convert.ToInt32(_config["JwtSettings:RefreshTokenExpiryDays"]);


        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        return new TokenDto
        {
            AccessToken = AccessToken,
            RefreshToken = refreshToken
        };
    }

    public Task<bool?> Logout(Guid userId, string refreshToken)
    {
        throw new NotImplementedException();
    }

    public async Task<TokenDto?> RefreshToken(RefreshTokenRequestDto request)
    {
        var oldRefreshTokenHash = await _refreshTokenRepository.GetByTokenHashAsync(BCrypt.Net.BCrypt.HashPassword(request.RefreshToken));

        if (oldRefreshTokenHash == null)
        {
            // LOG: Invalid refresh token attempt
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var refreshTokenExpiryDays = Convert.ToInt32(_config["JwtSettings:RefreshTokenExpiryDays"]);
        var expiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);

        string accessToken = GenerateJwtToken(oldRefreshTokenHash.User);
        string RefreshToken = GenerateRefreshToken();
        string newRefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(RefreshToken);

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = oldRefreshTokenHash.UserId,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = expiryTime,
            CreatedAt = DateTime.UtcNow,
        };

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
        await _refreshTokenRepository.RevokeTokenAsync(oldRefreshTokenHash.Id, newRefreshTokenEntity.Id);

        return new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = RefreshToken
        };
    }
}