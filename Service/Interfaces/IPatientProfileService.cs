using Service.DTOs;

namespace Service.Interfaces;

public interface IPatientProfileService
{
    Task<IEnumerable<PatientProfileDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<PatientProfileDto?> GetByIdAsync(Guid id);
    Task<PatientProfileDto> CreateAsync(CreatePatientProfileDto dto);
    Task<PatientProfileDto?> UpdateAsync(UpdatePatientProfileDto dto);
    Task DeleteAsync(Guid id);
    Task<PatientWithAppointmentsDto?> GetWithAppointmentsAsync(Guid id);
    Task<PatientWithMedicalRecordDto?> GetWithMedicalRecordsAsync(Guid id);
}
