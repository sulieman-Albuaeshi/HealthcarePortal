using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Repository.Interfaces;

namespace Repository.Implementations;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType)
    {
        return await _dbSet
            .Where(a => a.EntityType == entityType)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(a => a.Timestamp >= from && a.Timestamp <= to)
            .Include(a => a.User)
            .ToListAsync();
    }
}
