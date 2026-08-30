using FluentValidation;
using HelpDesk.Api.DTOs;

namespace HelpDesk.Api.Validators;

public class UpsertCategoryValidator : AbstractValidator<UpsertCategoryDto>
{
    public UpsertCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
