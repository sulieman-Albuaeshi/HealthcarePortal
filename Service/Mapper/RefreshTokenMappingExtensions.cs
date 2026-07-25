using Domain.Models;
using Service.DTOs;

namespace Service.Extensions;

public static class RefreshTokenMappingExtensions
{
    public static RefreshTokenDto ToDto(this RefreshToken token)
    {
        return new RefreshTokenDto
        {
            Id = token.Id,
            UserId = token.UserId,
            TokenHash = token.TokenHash,
            DeviceInfo = token.DeviceInfo,
            IpAddress = token.IpAddress,
            ExpiresAt = token.ExpiresAt,
            CreatedAt = token.CreatedAt,
            RevokedAt = token.RevokedAt,
            ReplacedByTokenId = token.ReplacedByTokenId,
            User = token.User?.ToDto()
        };
    }

    public static RefreshToken ToEntity(this RefreshTokenDto dto)
    {
        return new RefreshToken
        {
            Id = dto.Id,
            UserId = dto.UserId,
            TokenHash = dto.TokenHash,
            DeviceInfo = dto.DeviceInfo,
            IpAddress = dto.IpAddress,
            ExpiresAt = dto.ExpiresAt,
            CreatedAt = dto.CreatedAt,
            RevokedAt = dto.RevokedAt,
            ReplacedByTokenId = dto.ReplacedByTokenId
        };
    }
}
