using Domain.Models;
using Service.DTOs;

namespace Service.Extensions;

public static class MedicalRecordMappingExtensions
{
    public static MedicalRecordDto ToDto(this MedicalRecord record)
    {
        return new MedicalRecordDto
        {
            Id = record.Id,
            Title = record.Title,
            Description = record.Description,
            Type = record.Type,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            Doctor = record.Doctor?.ToDto(),
            Patient = record.Patient?.ToDto()
        };
    }
    public static MedicalRecord ToEntity(this CreateMedicalRecordDto dto)
    {
        return new MedicalRecord
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void PatchValuesFrom(this UpdateMedicalRecordDto dto, MedicalRecord entity)
    {
        if (!string.IsNullOrEmpty(dto.Title) && dto.Title != entity.Title)
            entity.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.Description) && dto.Description != entity.Description)
            entity.Description = dto.Description;
        if (dto.Type != entity.Type)
            entity.Type = dto.Type ?? Domain.Enums.RecordType.Note ;
        if (dto.IsDelete.HasValue && dto.IsDelete.Value != entity.IsDeleted)
            entity.IsDeleted = dto.IsDelete.Value;
        if (dto.UpdatedAt.HasValue && dto.UpdatedAt.Value != entity.UpdatedAt)
            entity.UpdatedAt = dto.UpdatedAt.Value;
    }
}