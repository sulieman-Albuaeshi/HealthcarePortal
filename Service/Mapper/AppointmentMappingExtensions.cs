using Domain.Enums;
using Domain.Models;
using Service.DTOs;

namespace Service.Extensions;

public static class AppointmentMappingExtensions
{
    public static AppointmentDto ToDto(this Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            DurationMinutes = appointment.DurationMinutes,
            Status = appointment.Status,
            Notes = appointment.Notes,
            CancellationReason = appointment.CancellationReason,
            CreatedAt = appointment.CreatedAt,
            ScheduledAt = appointment.ScheduledAt,
            Doctor = appointment.Doctor?.ToDto(),
            Patient = appointment.Patient?.ToDto()
        };
    }

    public static Appointment ToEntity(this CreateAppointmentDto dto)
    {
        return new Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            DurationMinutes = dto.DurationMinutes,
            Status = dto.Status,
            Notes = dto.Notes,
            ScheduledAt = dto.ScheduledAt,
            IsDelete = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void PatchValuesFrom(this UpdateAppointmentDto dto, Appointment entity)
    {
        if (dto.DurationMinutes.HasValue && dto.DurationMinutes.Value != entity.DurationMinutes)
            entity.DurationMinutes = dto.DurationMinutes.Value;

        if (dto.Status.HasValue && dto.Status.Value != entity.Status)
            entity.Status = dto.Status.Value;

        if (!string.IsNullOrEmpty(dto.Notes) && dto.Notes != entity.Notes)
            entity.Notes = dto.Notes;

        if (!string.IsNullOrEmpty(dto.CancellationReason) && dto.CancellationReason != entity.CancellationReason)
            entity.CancellationReason = dto.CancellationReason;

        if (dto.ScheduledAt.HasValue && dto.ScheduledAt.Value != entity.ScheduledAt)
            entity.ScheduledAt = dto.ScheduledAt.Value;

        if (dto.UpdatedAt != entity.UpdatedAt)
            entity.UpdatedAt = dto.UpdatedAt;

        if (dto.UpdatedBy != entity.UpdatedBy)
            entity.UpdatedBy = dto.UpdatedBy;

        if (dto.IsDelete.HasValue && dto.IsDelete.Value != entity.IsDelete)
            entity.IsDelete = dto.IsDelete.Value;
    }
}
