using FluentValidation;
using HelpDesk.Api.DTOs;

namespace HelpDesk.Api.Validators;

public class UpsertDepartmentValidator : AbstractValidator<UpsertDepartmentDto>
{
    public UpsertDepartmentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
