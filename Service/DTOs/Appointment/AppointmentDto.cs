using Domain.Enums;
using System.Text.Json.Serialization;

namespace Service.DTOs;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public int DurationMinutes { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DoctorProfileDto? Doctor { get; set; }
    public PatientProfileDto? Patient { get; set; }
}
