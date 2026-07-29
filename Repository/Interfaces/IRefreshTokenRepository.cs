using Domain.Models;

namespace Repository.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);
    Task RevokeTokenAsync(RefreshToken token, Guid? replacedByTokenId = null);
    Task<int> RevokeTokenFamilyIfOld(Guid tokenRootUserId);
    Task AddAsync(RefreshToken token);
    Task SaveChangesAsync();
}
