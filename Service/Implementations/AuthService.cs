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
using Service.Utility;
using BC = BCrypt.Net.BCrypt;

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
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };
        if (user.Role == UserRole.PatientAndDoctor)
        {
            claims.Add(new Claim(ClaimTypes.Role, nameof(UserRole.Patient)));
            claims.Add(new Claim(ClaimTypes.Role, nameof(UserRole.Doctor)));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
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
        var refreshTokenHash = TokenHasher.HashToken(refreshToken);
        var refreshTokenExpiryDays = Convert.ToInt32(_config["JwtSettings:RefreshTokenExpirationDays"]);


        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _refreshTokenRepository.SaveChangesAsync();

        return new TokenDto
        {
            AccessToken = AccessToken,
            RefreshToken = refreshToken
        };
    }
    public async Task<TokenDto?> Login(LoginDto userLoginDto)
    {
        var user = await _userRepository.GetByEmailAsync(userLoginDto.Email);

        if (user == null || !BC.Verify(userLoginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");
            
        return await GenerateTokensForUserAsync(user);
    }

    public Task<bool?> Logout(Guid userId, string refreshToken)
    {
        throw new NotImplementedException();
    }

    public async Task<TokenDto?> RefreshToken(RefreshTokenRequestDto request)
    {
        var oldRefreshToken = await _refreshTokenRepository.GetByTokenHashAsync(TokenHasher.HashToken(request.RefreshToken));
        
        if (oldRefreshToken == null)
        {
            // LOG: Invalid refresh token attempt
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if(oldRefreshToken.User == null)
            throw new UnauthorizedAccessException("User account is inactive or not found.");

        if (oldRefreshToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Token expired.");

        if (oldRefreshToken.RevokedAt != null)
        {
            // THEFT DETECTED: Revoke the entire token family!
            await _refreshTokenRepository.RevokeTokenFamilyIfOld(oldRefreshToken.UserId);
            await _refreshTokenRepository.SaveChangesAsync();
            throw new UnauthorizedAccessException("Token reuse detected. All sessions revoked.");
        }

        string accessToken = GenerateJwtToken(oldRefreshToken.User);
        string rawRefreshToken = GenerateRefreshToken();
        string newRefreshTokenHash = TokenHasher.HashToken(rawRefreshToken);
        var refreshTokenExpiryDays = Convert.ToInt32(_config["JwtSettings:RefreshTokenExpirationDays"]);

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = oldRefreshToken.UserId,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
        };

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
        await _refreshTokenRepository.RevokeTokenAsync(oldRefreshToken, newRefreshTokenEntity.Id);
        await _refreshTokenRepository.SaveChangesAsync();

        return new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken
        };
    }

    public async Task<TokenDto?> RegisterPatientAsync(RegisterPatientDto dto)
    {
        // Check if email is already registered
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        
        // Hash the password
        if (existingUser == null)
        {
            var passwordHash = BC.HashPassword(dto.Password);        
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
                if (!BC.Verify(dto.Password, existingUser.PasswordHash))
                    throw new UnauthorizedAccessException("invalid credentials");
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
            User = existingUser,
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
        var passwordHash = BC.HashPassword(dto.Password);

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
            User = user,
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
