using Domain.Models;

namespace Repository.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);
    Task RevokeTokenAsync(Guid tokenId, Guid? replacedByTokenId = null);
    Task<int> RevokeTokenFamilyIfOld(Guid tokenRootId);
    Task AddAsync(RefreshToken token);
}
