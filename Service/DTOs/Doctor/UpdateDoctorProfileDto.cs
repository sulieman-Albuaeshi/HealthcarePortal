namespace Service.DTOs;

public class UpdateDoctorProfileDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public string LicenseNumber { get; set; } = null!;
    public bool IsDeleted { get; set; }
}