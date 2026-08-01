using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Repository.Interfaces;
using Repository.Data;

namespace Repository.Implementations;

public class DoctorProfileRepository : GenericRepository<DoctorProfile>, IDoctorProfileRepository
{
    public DoctorProfileRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<DoctorProfile?> GetWithAppointmentsAsync(Guid id)
    {
        return await _dbSet
            .Include(d => d.Appointments)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DoctorProfile?> GetWithMedicalRecordsAsync(Guid id)
    {
        return await _dbSet
            .Include(d => d.MedicalRecords)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<DoctorProfile>> GetBySpecializationAsync(string specialization)
    {
        return await _dbSet
            .Where(d => d.Specialization == specialization)
            .ToListAsync();
    }
}
