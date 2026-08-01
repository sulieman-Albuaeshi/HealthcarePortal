using FluentValidation;
using Service.DTOs;

namespace API.Validation;

public class UpdatePatientProfileDtoValidator : AbstractValidator<UpdatePatientProfileDto>
{
    public UpdatePatientProfileDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("Patient profile ID is required");

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
