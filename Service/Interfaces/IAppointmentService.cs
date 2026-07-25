using Service.DTOs;

namespace Service.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<AppointmentDto?> GetByIdAsync(Guid id);
    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
    Task<AppointmentDto?> UpdateAsync(UpdateAppointmentDto dto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<AppointmentDto>> GetByPatientIdAsync(Guid patientId);
    Task<IEnumerable<AppointmentDto>> GetByDoctorIdAsync(Guid doctorId);
    Task<IEnumerable<AppointmentDto>> GetByDateAsync(DateTime date);
    Task<IEnumerable<AppointmentDto>> GetUpcomingAppointmentsAsync();
}
