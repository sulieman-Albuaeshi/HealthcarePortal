using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Repository.Interfaces;

namespace Repository.Implementations;

public class DoctorProfileRepository : GenericRepository<DoctorProfile>, IDoctorProfileRepository
{
    public DoctorProfileRepository(DbContext context) : base(context)
    {
    }

    public async Task<DoctorProfile?> GetByUserIdAsync(Guid userId)
    {
       return await _dbSet.FirstOrDefaultAsync(d => d.UserId == userId);
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

    public async Task<Guid> GetUserIDByDoctorIdAsync(Guid doctorProfileId)
    {
        var doctorProfile = await _dbSet.FirstOrDefaultAsync(d => d.Id == doctorProfileId);
        if (doctorProfile == null)
        {
            throw new InvalidOperationException($"Doctor profile with ID {doctorProfileId} not found.");
        }
        return doctorProfile.UserId;
    }
}
