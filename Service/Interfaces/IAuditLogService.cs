using Service.DTOs;

namespace Service.Interfaces;

public interface IAuditLogService
{
    Task<IEnumerable<AuditLogDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<AuditLogDto?> GetByIdAsync(Guid id);
    Task<AuditLogDto> CreateAsync(AuditLogDto dto);
    Task<IEnumerable<AuditLogDto>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<AuditLogDto>> GetByEntityTypeAsync(string entityType);
    Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime from, DateTime to);
}
