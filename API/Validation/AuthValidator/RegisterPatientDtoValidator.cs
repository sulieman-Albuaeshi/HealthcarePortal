using FluentValidation;
using Service.DTOs;

namespace API.Validation;

public class RegisterPatientDtoValidator : AbstractValidator<RegisterPatientDto>
{
    public RegisterPatientDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob != default).WithMessage("Date of birth is required")
            .Must(dob => dob <= DateOnly.FromDateTime(DateTime.Today)).WithMessage("Date of birth cannot be in the future");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .Matches(@"^\d+$").WithMessage("Phone number must contain only digits")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.EmergencyContact)
            .MaximumLength(200).WithMessage("Emergency contact must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.EmergencyContact));
    }
}
