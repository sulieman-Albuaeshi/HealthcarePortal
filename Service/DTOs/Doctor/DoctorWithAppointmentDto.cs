namespace Service.DTOs;

public class DoctorWithAppointmentDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public string LicenseNumber { get; set; } = null!;
    public IEnumerable<AppointmentDto> Appointments { get; set; } = new List<AppointmentDto>();
}
