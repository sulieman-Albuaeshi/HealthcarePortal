using Service.DTOs;

namespace Service.Interfaces;

public interface IDoctorProfileService
{
    Task<IEnumerable<DoctorProfileDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<DoctorProfileDto?> GetByIdAsync(Guid id);
    Task<DoctorProfileDto> CreateAsync(CreateDoctorProfileDto dto);
    Task<DoctorProfileDto?> UpdateAsync(UpdateDoctorProfileDto dto);
    Task DeleteAsync(Guid id);
    Task<DoctorProfileDto?> GetByUserIdAsync(Guid userId);
    Task<DoctorWithAppointmentDto?> GetWithAppointmentsAsync(Guid id);
    Task<DoctorWithMedicalRecordDto?> GetWithMedicalRecordsAsync(Guid id);
    Task<IEnumerable<DoctorProfileDto>> GetBySpecializationAsync(string specialization);
    Task<Guid> GetUserIDByDoctorIdAsync(Guid doctorProfileId);
}
