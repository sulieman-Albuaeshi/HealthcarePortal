using Domain.Models;

namespace Repository.Interfaces;

public interface IDoctorProfileRepository : IGenericRepository<DoctorProfile>
{
    Task<DoctorProfile?> GetByUserIdAsync(Guid userId);
    Task<DoctorProfile?> GetWithAppointmentsAsync(Guid id);
    Task<DoctorProfile?> GetWithMedicalRecordsAsync(Guid id);
    Task<IEnumerable<DoctorProfile>> GetBySpecializationAsync(string specialization);
    Task<Guid> GetUserIDByDoctorIdAsync(Guid doctorProfileId);
}
