using FluentValidation;
using Service.DTOs;

namespace API.Validation;

public class CreateAppointmentDtoValidator : AbstractValidator<CreateAppointmentDto>
{
    public CreateAppointmentDtoValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEqual(Guid.Empty).WithMessage("Patient ID is required");

        RuleFor(x => x.DoctorId)
            .NotEqual(Guid.Empty).WithMessage("Doctor ID is required");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid appointment status");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.ScheduledAt)
            .Must(s => s != default).WithMessage("Scheduled date and time is required")
            .Must(s => s > DateTime.Now).WithMessage("Scheduled date and time must be in the future");
    }
}
