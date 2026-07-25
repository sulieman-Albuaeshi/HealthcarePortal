using Service.DTOs;

namespace Service.Interfaces;

public interface IMedicalRecordService
{
    Task<IEnumerable<MedicalRecordDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<MedicalRecordDto?> GetByIdAsync(Guid id);
    Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto);
    Task<MedicalRecordDto?> UpdateAsync(UpdateMedicalRecordDto dto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<MedicalRecordDto>> GetByPatientIdAsync(Guid patientId);
    Task<IEnumerable<MedicalRecordDto>> GetByDoctorIdAsync(Guid doctorId);
    Task<IEnumerable<MedicalRecordDto>> GetByTitleAsync(string title);
}
