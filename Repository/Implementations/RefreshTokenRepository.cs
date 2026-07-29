using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Repository.Interfaces;
using Repository.Data;  

namespace Repository.Implementations;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
        .Include(t => t.User)
        .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId)
    {
        return await  _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.Now)
            .Include(t => t.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId)
    {
        return await  _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.Now)
            .Include(t => t.User)
            .ToListAsync();
    }

    public async Task RevokeTokenAsync(RefreshToken token, Guid? replacedByTokenId = null)
    {
        if (token != null)
        {
            token.TokenHash = null;
            token.RevokedAt = DateTime.Now;
            token.ReplacedByTokenId = replacedByTokenId;
        }
    }

    public async Task<int> RevokeTokenFamilyIfOld(Guid tokenRootUserId) 
    {
        if (tokenRootUserId == Guid.Empty)
            return 0;

        return await  _context.RefreshTokens
            .Where(t => t.UserId == tokenRootUserId && t.RevokedAt == null)
            .ExecuteUpdateAsync(t => t
                .SetProperty(token => token.RevokedAt, DateTime.UtcNow)
                .SetProperty(token => token.TokenHash, (string?)null)
            ); 
    }

    public async Task AddAsync(RefreshToken token)
    {
        await  _context.RefreshTokens.AddAsync(token);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
