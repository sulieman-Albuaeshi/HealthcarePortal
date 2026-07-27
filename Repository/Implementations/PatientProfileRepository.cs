using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Repository.Interfaces;
using Repository.Data;

namespace Repository.Implementations;

public class PatientProfileRepository : GenericRepository<PatientProfile>, IPatientProfileRepository
{
    public PatientProfileRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PatientProfile?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<PatientProfile?> GetWithAppointmentsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PatientProfile?> GetWithMedicalRecordsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.MedicalRecords)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Guid> GetUserIDByPatientIdAsync(Guid patientProfileId)
    {
        var patientProfile = await _dbSet.FirstOrDefaultAsync(p => p.Id == patientProfileId);
        if (patientProfile == null)
        {
            throw new KeyNotFoundException($"Patient profile with ID {patientProfileId} not found.");
        }
        return patientProfile.UserId;
    }
}
