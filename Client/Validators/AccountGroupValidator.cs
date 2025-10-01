using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators;

public class AccountGroupValidator : AbstractValidator<AccountGroup>
{
    public AccountGroupValidator(IStringLocalizer localizer)
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage(localizer["NameRequired"])
            .MaximumLength(100).WithMessage(localizer["NameMaxLength"]);

        RuleFor(c => c.Code)
            .MaximumLength(20).WithMessage(localizer["CodeMaxLength"]);

        RuleFor(c => c.Nature)
            .IsInEnum().WithMessage(localizer["InvalidAccountNature"]);
    }
}
