using FluentValidation;
using HelpDesk.Api.DTOs.Tickets;

namespace HelpDesk.Api.Validators.Tickets;

public class CreateTicketValidator : AbstractValidator<CreateTicketDto>
{
    public CreateTicketValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(10);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Location).MaximumLength(200);
    }
}
