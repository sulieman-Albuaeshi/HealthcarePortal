using Domain.Models;
using Service.DTOs;

namespace Service.Extensions;

public static class DoctorProfileMappingExtensions
{
    public static DoctorProfileDto ToDto(this DoctorProfile profile)
    {
        return new DoctorProfileDto
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Specialization = profile.Specialization,
            LicenseNumber = profile.LicenseNumber,
        };
    }
    public static DoctorProfile ToEntity(this CreateDoctorProfileDto dto)
    {
        return new DoctorProfile
        {
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Specialization = dto.Specialization,
            LicenseNumber = dto.LicenseNumber,
            IsDeleted = false
        };
    }
    public static void PatchValuesFrom(this UpdateDoctorProfileDto dto, DoctorProfile entity)
    {
        if (!string.IsNullOrEmpty(dto.FirstName) && dto.FirstName != entity.FirstName)
            entity.FirstName = dto.FirstName;
        if (!string.IsNullOrEmpty(dto.LastName) && dto.LastName != entity.LastName)
            entity.LastName = dto.LastName;
        if (!string.IsNullOrEmpty(dto.Specialization) && dto.Specialization != entity.Specialization)
            entity.Specialization = dto.Specialization;
        if (!string.IsNullOrEmpty(dto.LicenseNumber) && dto.LicenseNumber != entity.LicenseNumber)
            entity.LicenseNumber = dto.LicenseNumber;
        if (dto.IsDeleted != entity.IsDeleted)
            entity.IsDeleted = dto.IsDeleted;
    }
}