using Domain.Models;

namespace Repository.Interfaces;

public interface IDoctorProfileRepository : IGenericRepository<DoctorProfile>
{
    Task<DoctorProfile?> GetWithAppointmentsAsync(Guid id);
    Task<DoctorProfile?> GetWithMedicalRecordsAsync(Guid id);
    Task<IEnumerable<DoctorProfile>> GetBySpecializationAsync(string specialization);
}
