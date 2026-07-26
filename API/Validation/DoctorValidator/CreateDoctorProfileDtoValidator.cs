using FluentValidation;
using Service.DTOs;

namespace API.Validation;

public class CreateDoctorProfileDtoValidator : AbstractValidator<CreateDoctorProfileDto>
{
    public CreateDoctorProfileDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty).WithMessage("User ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Specialization is required")
            .MaximumLength(150).WithMessage("Specialization must not exceed 150 characters");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("License number is required")
            .MaximumLength(100).WithMessage("License number must not exceed 100 characters");
    }
}
