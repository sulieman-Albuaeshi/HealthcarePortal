using Domain.Models;

namespace Repository.Interfaces;

public interface IPatientProfileRepository : IGenericRepository<PatientProfile>
{
    Task<PatientProfile?> GetByUserIdAsync(Guid userId);
    Task<PatientProfile?> GetWithAppointmentsAsync(Guid id);
    Task<PatientProfile?> GetWithMedicalRecordsAsync(Guid id);
}
