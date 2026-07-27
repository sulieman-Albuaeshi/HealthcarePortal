using Domain.Models;
using Repository.Interfaces;
using Service.DTOs;
using Service.Extensions;
using Service.Interfaces;

namespace Service.Implementations;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }
    
    public async Task<IEnumerable<AuditLogDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var logs = await _auditLogRepository.GetAllAsync(pageNumber, pageSize);
        return logs.Select(l => l.ToDto());
    }

    public async Task<AuditLogDto?> GetByIdAsync(Guid id)
    {
        var log = await _auditLogRepository.GetByIdAsync(id);
        return log?.ToDto();
    }

    public async Task<AuditLogDto> CreateAsync(AuditLogDto dto)
    {
        var entity = dto.ToEntity();
        await _auditLogRepository.AddAsync(entity);
        return entity.ToDto();
    }

    public async Task<IEnumerable<AuditLogDto>> GetByUserIdAsync(Guid userId)
    {
        var logs = await _auditLogRepository.GetByUserIdAsync(userId);
        return logs.Select(l => l.ToDto());
    }

    public async Task<IEnumerable<AuditLogDto>> GetByEntityTypeAsync(string entityType)
    {
        var logs = await _auditLogRepository.GetByEntityTypeAsync(entityType);
        return logs.Select(l => l.ToDto());
    }

    public async Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var logs = await _auditLogRepository.GetByDateRangeAsync(from, to);
        return logs.Select(l => l.ToDto());
    }
}
