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
        return await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.RevokedAt == null && t.ExpiresAt < DateTime.Now);
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

    public async Task RevokeTokenAsync(Guid tokenId, Guid? replacedByTokenId = null)
    {
        var token = await  _context.RefreshTokens.FindAsync(tokenId);
        if (token != null)
        {
            token.TokenHash = null;
            token.RevokedAt = DateTime.Now;
            token.ReplacedByTokenId = replacedByTokenId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> RevokeTokenFamilyIfOld(Guid tokenRootId)
    {
        var targetToken = await  _context.RefreshTokens.Where(t => t.Id == tokenRootId && t.ReplacedByTokenId != null).FirstOrDefaultAsync();

        if (targetToken == null)
            return 0;

        return await  _context.RefreshTokens
            .Where(t => t.UserId == targetToken.UserId && t.RevokedAt == null)
            .ExecuteUpdateAsync(t => t
                .SetProperty(token => token.RevokedAt, DateTime.UtcNow)
                .SetProperty(token => token.TokenHash, (string?)null)
            ); 
    }

    public async Task AddAsync(RefreshToken token)
    {
        await  _context.RefreshTokens.AddAsync(token);
    }
}
