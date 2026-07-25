using Domain.Models;
using Repository.Interfaces;
using Service.DTOs;
using Service.Extensions;
using Service.Interfaces;

namespace Service.Implementations;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<RefreshTokenDto?> GetByTokenHashAsync(string tokenHash)
    {
        var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);
        return token?.ToDto();
    }

    public async Task<IEnumerable<RefreshTokenDto>> GetByUserIdAsync(Guid userId)
    {
        var tokens = await _refreshTokenRepository.GetByUserIdAsync(userId);
        return tokens.Select(t => t.ToDto());
    }

    public async Task<IEnumerable<RefreshTokenDto>> GetActiveTokensByUserIdAsync(Guid userId)
    {
        var tokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);
        return tokens.Select(t => t.ToDto());
    }

    public async Task RevokeTokenAsync(Guid tokenId, Guid? replacedByTokenId = null)
    {
        await _refreshTokenRepository.RevokeTokenAsync(tokenId, replacedByTokenId);
    }

    public async Task<int> RevokeTokenFamilyIfOldAsync(Guid tokenRootId)
    {
        return await _refreshTokenRepository.RevokeTokenFamilyIfOld(tokenRootId);
    }

    public async Task AddAsync(RefreshTokenDto dto)
    {
        var entity = dto.ToEntity();
        await _refreshTokenRepository.AddAsync(entity);
    }
}
