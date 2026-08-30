using FluentValidation;
using HelpDesk.Api.DTOs.Tickets;

namespace HelpDesk.Api.Validators.Tickets;

public class CreateCommentValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
    }
}
