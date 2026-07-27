using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Repository.Interfaces;
using Repository.Data;

namespace Repository.Implementations;

public class MedicalRecordRepository : GenericRepository<MedicalRecord>, IMedicalRecordRepository
{
    public MedicalRecordRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(Guid patientId)
    {
        return await _dbSet
            .Where(m => m.PatientId == patientId)
            .Include(m => m.Patient)
            .Include(m => m.Doctor)
            .ToListAsync();
    }

    public async Task<IEnumerable<MedicalRecord>> GetByDoctorIdAsync(Guid doctorId)
    {
        return await _dbSet
            .Where(m => m.DoctorId == doctorId)
            .Include(m => m.Patient)
            .Include(m => m.Doctor)
            .ToListAsync();
    }

    public async Task<IEnumerable<MedicalRecord>> GetByTitleAsync(string title)
    {
        return await _dbSet
            .Where(m => m.Title.Contains(title))
            .Include(m => m.Patient)
            .Include(m => m.Doctor)
            .ToListAsync();
    }
}
