using FluentValidation;
using Service.DTOs;

namespace API.Validation;

public class UpdateAppointmentDtoValidator : AbstractValidator<UpdateAppointmentDto>
{
    public UpdateAppointmentDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("Appointment ID is required");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes")
            .When(x => x.DurationMinutes.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid appointment status")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.CancellationReason)
            .MaximumLength(500).WithMessage("Cancellation reason must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.CancellationReason));

        RuleFor(x => x.ScheduledAt)
            .Must(s => s > DateTime.Now).WithMessage("Scheduled date and time must be in the future")
            .When(x => x.ScheduledAt.HasValue);

        RuleFor(x => x.UpdatedBy)
            .NotEqual(Guid.Empty).WithMessage("Updated by user ID is required");
    }
}
