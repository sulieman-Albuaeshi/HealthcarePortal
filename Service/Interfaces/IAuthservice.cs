using Service.DTOs;

namespace Service.Interfaces;

public interface IAuthService
{
    public Task<TokenDto?> Login(LoginDto userLoginDto);
    public Task<TokenDto?> RefreshToken(RefreshTokenRequestDto request);
    public Task<bool?> Logout(Guid userId, string refreshToken);
    
}