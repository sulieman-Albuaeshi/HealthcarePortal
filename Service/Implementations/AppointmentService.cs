using Domain.Models;
using Repository.Interfaces;
using Service.DTOs;
using Service.Extensions;
using Service.Interfaces;

namespace Service.Implementations;

public class AppointmentService :  IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }
    public async Task<IEnumerable<AppointmentDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var appointments = await _appointmentRepository.GetAllAsync(pageNumber, pageSize);
        return appointments.Select(a => a.ToDto());
    }

    public async Task<AppointmentDto?> GetByIdAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        return appointment?.ToDto();
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
    {
        var entity = dto.ToEntity();
        await _appointmentRepository.AddAsync(entity);
        await _appointmentRepository.SaveChangesAsync();
        return entity.ToDto();
    }
    public async Task DeleteAsync(Guid id)
    {
        await _appointmentRepository.DeleteAsync(id);
        await _appointmentRepository.SaveChangesAsync();
    }

    public async Task<AppointmentDto?> UpdateAsync(UpdateAppointmentDto dto)
    {
        var entity = await _appointmentRepository.GetByIdAsync(dto.Id);
        if (entity == null)
            return null;
        dto.PatchValuesFrom(entity);
        await _appointmentRepository.UpdateAsync(entity);
        await _appointmentRepository.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task<IEnumerable<AppointmentDto>> GetByPatientIdAsync(Guid patientId)
    {
        var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);
        return appointments.Select(a => a.ToDto());
    }

    public async Task<IEnumerable<AppointmentDto>> GetByDoctorIdAsync(Guid doctorId)
    {
        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId);
        return appointments.Select(a => a.ToDto());
    }

    public async Task<IEnumerable<AppointmentDto>> GetByDateAsync(DateTime date)
    {
        var appointments = await _appointmentRepository.GetByDateAsync(date);
        return appointments.Select(a => a.ToDto());
    }

    public async Task<IEnumerable<AppointmentDto>> GetUpcomingAppointmentsAsync()
    {
        var appointments = await _appointmentRepository.GetUpcomingAppointmentsAsync();
        return appointments.Select(a => a.ToDto());
    }

    public async Task<AppointmentDto?> GetByIdWithDoctorAndPatientAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdWithDoctorAndPatientAsync(id);
        return appointment?.ToDto();
    }
}
