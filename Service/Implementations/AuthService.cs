using Service.DTOs;
using Service.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Domain.Models;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

using Repository.Interfaces;

namespace Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IDoctorProfileRepository _doctorProfileRepository;
    private readonly IPatientProfileRepository _patientProfileRepository;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IDoctorProfileRepository doctorProfileRepository,
        IPatientProfileRepository patientProfileRepository,
        IConfiguration config)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _doctorProfileRepository = doctorProfileRepository;
        _patientProfileRepository = patientProfileRepository;
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

    private async Task<TokenDto> GenerateTokensForUserAsync(User user)
    {
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
    public async Task<TokenDto?> Login(LoginDto userLoginDto)
    {
        var user = await _userRepository.GetByEmailAsync(userLoginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");
            
        return await GenerateTokensForUserAsync(user);
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

    public async Task<TokenDto?> RegisterPatientAsync(RegisterPatientDto dto)
    {
        // Check if email is already registered
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        
        // Hash the password
        if (existingUser == null)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);        
            // Create the user
            existingUser = new User
            {
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = UserRole.Patient,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            await _userRepository.AddAsync(existingUser);
        }
        else 
        {
            if (existingUser.Role == UserRole.Doctor)
            {
                existingUser.Role = UserRole.PatientAndDoctor;
                await _userRepository.UpdateAsync(existingUser);
            }
            else if (existingUser.Role == UserRole.PatientAndDoctor || existingUser.Role == UserRole.Patient)
            {   
                // User ALREADY has a patient profile
                return null;
            }
        }

        // Create the patient profile
        var patientProfile = new PatientProfile
        {
            UserId = existingUser.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber,
            EmergencyContact = dto.EmergencyContact,
            IsDeleted = false,
        };

        await _patientProfileRepository.AddAsync(patientProfile);
        await _patientProfileRepository.SaveChangesAsync();
        return  await GenerateTokensForUserAsync(existingUser);
    }

    public async Task<TokenDto?> RegisterDoctorAsync(RegisterDoctorDto dto)
    {
        // Check if email is already registered
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            return null;

        // Hash the password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Create the user
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = passwordHash,
            Role = UserRole.Doctor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await _userRepository.AddAsync(user);

        // Create the doctor profile
        var doctorProfile = new DoctorProfile
        {
            UserId = user.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Specialization = dto.Specialization,
            LicenseNumber = dto.LicenseNumber,
            IsDeleted = false,
        };

        await _doctorProfileRepository.AddAsync(doctorProfile);
        await _doctorProfileRepository.SaveChangesAsync();
        return await GenerateTokensForUserAsync(user);
    }
}
