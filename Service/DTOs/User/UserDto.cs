using Domain.Enums;
using System.Text.Json.Serialization;

namespace Service.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DoctorProfileDto? DoctorProfile { get; set; }
    public PatientProfileDto? PatientProfile { get; set; }
}