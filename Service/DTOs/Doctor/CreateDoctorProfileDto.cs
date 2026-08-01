using Service.DTOs;
namespace Service.DTOs;

public class CreateDoctorProfileDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public string LicenseNumber { get; set; } = null!;
}
