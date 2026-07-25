using Domain.Enums;
using System.Text.Json.Serialization;

namespace Service.DTOs;

public class CreateAppointmentDto
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public int DurationMinutes { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime ScheduledAt { get; set; }
}
