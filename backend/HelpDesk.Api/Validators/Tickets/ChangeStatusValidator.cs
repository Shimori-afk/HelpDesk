using FluentValidation;
using HelpDesk.Api.DTOs.Tickets;

namespace HelpDesk.Api.Validators.Tickets;

public class ChangeStatusValidator : AbstractValidator<ChangeStatusDto>
{
    public ChangeStatusValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class ChangePriorityValidator : AbstractValidator<ChangePriorityDto>
{
    public ChangePriorityValidator()
    {
        RuleFor(x => x.Priority).IsInEnum();
    }
}
