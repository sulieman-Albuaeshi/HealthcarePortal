using Domain.Enums;

namespace Domain.Models;
public partial class Appointment
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
    public bool IsDelete { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public virtual DoctorProfile Doctor { get; set; } = null!;
    public virtual PatientProfile Patient { get; set; } = null!;
    public virtual User? UpdatedByNavigation { get; set; }
}
