using FluentValidation;
using Service.DTOs;

namespace API.Validation;

public class UpdateMedicalRecordDtoValidator : AbstractValidator<UpdateMedicalRecordDto>
{
    public UpdateMedicalRecordDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("Medical record ID is required");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid record type")
            .When(x => x.Type.HasValue);
    }
}
