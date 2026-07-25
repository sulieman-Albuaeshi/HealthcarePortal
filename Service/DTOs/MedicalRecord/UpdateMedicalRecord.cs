using Domain.Enums;
using System.Text.Json.Serialization;

namespace Service.DTOs;

public class UpdateMedicalRecordDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; } 
    public string? Description { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RecordType? Type { get; set; }
    public bool? IsDelete { get; set; }
    public DateTime? UpdatedAt { get; set; }
}