using Domain.Models;
using Service.DTOs;

namespace Service.Extensions;

public static class AuditLogMappingExtensions
{
    public static AuditLogDto ToDto(this AuditLog auditLog)
    {
        return new AuditLogDto
        {
            Id = auditLog.Id,
            UserId = auditLog.UserId,
            Action = auditLog.Action,
            EntityType = auditLog.EntityType,
            EntityId = auditLog.EntityId,
            Details = auditLog.Details,
            Timestamp = auditLog.Timestamp,
            IpAddress = auditLog.IpAddress,
            User = auditLog.User?.ToUserAuditDto()
        };
    }

    public static AuditLog ToEntity(this AuditLogDto dto)
    {
        return new AuditLog
        {
            Id = dto.Id,
            UserId = dto.UserId,
            Action = dto.Action,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            Details = dto.Details,
            Timestamp = dto.Timestamp,
            IpAddress = dto.IpAddress
        };
    }
}
