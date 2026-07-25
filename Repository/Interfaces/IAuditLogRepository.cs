using Domain.Models;

namespace Repository.Interfaces;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to);
}
