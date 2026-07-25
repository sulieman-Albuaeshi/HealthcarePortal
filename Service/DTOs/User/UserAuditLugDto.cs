using Domain.Enums;
using System.Text.Json.Serialization;


namespace Service.DTOs;

public class UserAuditDTO
{
    public string Email { get; set; } = null!;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; }
}