using Service.DTOs;

namespace Service.Interfaces;

public interface IAuthService
{
    public Task<TokenDto?> Login(LoginDto userLoginDto);
    public Task<TokenDto?> RefreshToken(RefreshTokenRequestDto request);
    public Task<bool?> Logout(Guid userId, string refreshToken);
    public Task<TokenDto?> RegisterPatientAsync(RegisterPatientDto dto);
    public Task<TokenDto?> RegisterDoctorAsync(RegisterDoctorDto dto);
}
