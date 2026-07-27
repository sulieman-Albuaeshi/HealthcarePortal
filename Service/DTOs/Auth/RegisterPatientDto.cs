namespace Service.DTOs;

public class RegisterPatientDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmergencyContact { get; set; }
}
