using Domain.Models;
using Service.DTOs;

namespace Service.Extensions;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            DoctorProfile = user.DoctorProfile?.ToDto(),
            PatientProfile = user.PatientProfile?.ToDto()
        };
    }

    public static UserAuditDTO ToUserAuditDto(this User user)
    {
        return new UserAuditDTO
        {
            Email = user.Email,
            Role = user.Role
        };
    }

    public static User ToEntity(this CreateUserDto dto)
    {
        return new User
        {
            Email = dto.Email,
            PasswordHash = dto.PasswordHash,
            Role = dto.Role
        };
    }

    public static void PatchValuesFrom(this UserDto dto, User entity)
    {
        if (dto.PasswordHash != entity.PasswordHash)
            entity.PasswordHash = dto.PasswordHash;
        if (dto.Role != entity.Role)
            entity.Role = dto.Role;
        if (dto.IsActive != entity.IsActive)
            entity.IsActive = dto.IsActive;
        if (dto.UpdatedAt.HasValue && dto.UpdatedAt.Value != entity.UpdatedAt)
            entity.UpdatedAt = dto.UpdatedAt.Value;
    }
}
