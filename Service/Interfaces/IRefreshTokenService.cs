using Service.DTOs;

namespace Service.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshTokenDto?> GetByTokenHashAsync(string tokenHash);
    Task<IEnumerable<RefreshTokenDto>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<RefreshTokenDto>> GetActiveTokensByUserIdAsync(Guid userId);
    Task RevokeTokenAsync(Guid tokenId, Guid? replacedByTokenId = null);
    Task<int> RevokeTokenFamilyIfOldAsync(Guid tokenRootId);
    Task AddAsync(RefreshTokenDto dto);
}
