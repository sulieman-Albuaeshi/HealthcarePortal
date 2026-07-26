using Domain.Enums;
using System.Text.Json.Serialization;

namespace Service.DTOs;

public class UpdateUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; }
}
