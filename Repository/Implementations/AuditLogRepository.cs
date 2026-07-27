using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Repository.Interfaces;
using Repository.Data;

namespace Repository.Implementations;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync(int pageNumber, int pageSize)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _context.AuditLogs
            .Where(a => a.Timestamp >= from && a.Timestamp <= to)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<AuditLog> AddAsync(AuditLog entity)
    {
        _context.AuditLogs.Add(entity);
        return await _context.SaveChangesAsync() > 0 ? entity : null!; 
    }
}
