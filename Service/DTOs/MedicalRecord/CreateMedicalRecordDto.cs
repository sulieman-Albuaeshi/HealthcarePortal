using Domain.Enums;
using System.Text.Json.Serialization;

namespace Service.DTOs;

public class CreateMedicalRecordDto
{
    public Guid PatientId { get; set; }
    public Guid? DoctorId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RecordType Type { get; set; }
    public DateTime RecordDate { get; set; }
}
