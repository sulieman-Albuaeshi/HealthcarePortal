using Domain.Enums;
using System.Text.Json.Serialization;

namespace Service.DTOs;

public class MedicalRecordDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RecordType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DoctorProfileDto? Doctor { get; set; }
    public PatientProfileDto? Patient { get; set; }
}
