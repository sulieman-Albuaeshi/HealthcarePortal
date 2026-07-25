using Domain.Enums;
using System.Text.Json.Serialization;

namespace Service.DTOs;

public class UpdateAppointmentDto
{
    public Guid Id { get; set; }
    public int? DurationMinutes { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AppointmentStatus? Status { get; set; }
    public bool? IsDelete { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}