using Domain.Models;

namespace Repository.Interfaces;

public interface IMedicalRecordRepository : IGenericRepository<MedicalRecord>
{
    Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(Guid patientId);
    Task<IEnumerable<MedicalRecord>> GetByDoctorIdAsync(Guid doctorId);
    Task<IEnumerable<MedicalRecord>> GetByTitleAsync(string title);
}
