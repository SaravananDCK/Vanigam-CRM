using FluentValidation;

using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class ResetPasswordValidator : AbstractValidator<ApplicationUser>
    {
        public ResetPasswordValidator(IStringLocalizer localizer)
        {
            RuleFor(u => u.Email)
                .NotEmpty().WithMessage(localizer["EmailRequired"])
                .EmailAddress().WithMessage(localizer["ProvideValidEmail"]);
        }
    }
}

