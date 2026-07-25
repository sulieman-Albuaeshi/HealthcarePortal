using Domain.Models;
using Service.DTOs;

namespace Service.Extensions;

public static class PatientProfileMappingExtensions
{
    public static PatientProfileDto ToDto(this PatientProfile profile)
    {
        return new PatientProfileDto
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            DateOfBirth = profile.DateOfBirth,
            PhoneNumber = profile.PhoneNumber,
            EmergencyContact = profile.EmergencyContact,
        };
    }

    public static PatientProfile ToEntity(this CreatePatientProfileDto dto)
    {
        return new PatientProfile
        {
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber,
            EmergencyContact = dto.EmergencyContact,
            IsDeleted = false
        };
    }

    public static void PatchValuesFrom(this UpdatePatientProfileDto dto, PatientProfile entity)
    {
        if (dto.FirstName != entity.FirstName)
            entity.FirstName = dto.FirstName;
        if (dto.LastName != entity.LastName)
            entity.LastName = dto.LastName;
        if (dto.DateOfBirth != entity.DateOfBirth)
            entity.DateOfBirth = dto.DateOfBirth;
        if (dto.PhoneNumber != entity.PhoneNumber)
            entity.PhoneNumber = dto.PhoneNumber;
        if (dto.EmergencyContact != entity.EmergencyContact)
            entity.EmergencyContact = dto.EmergencyContact;
        if (dto.IsDeleted != entity.IsDeleted)
            entity.IsDeleted = dto.IsDeleted;   
    }
}