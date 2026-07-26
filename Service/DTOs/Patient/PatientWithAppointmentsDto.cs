namespace Service.DTOs;

public class PatientWithAppointmentsDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmergencyContact { get; set; }
    public IEnumerable<AppointmentDto> Appointments { get; set; } = new List<AppointmentDto>();
}
