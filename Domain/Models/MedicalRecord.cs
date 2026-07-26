using Domain.Enums;

namespace Domain.Models;

public partial class MedicalRecord
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public Guid? DoctorId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public RecordType Type { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual DoctorProfile? Doctor { get; set; }

    public virtual PatientProfile Patient { get; set; } = null!;
}
