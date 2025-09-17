using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects;

namespace Vanigam.CRM.Client.Validators
{
    public class ApplicationRoleValidator : AbstractValidator<ApplicationRole>
    {
        public ApplicationRoleValidator(IStringLocalizer localizer)
        {
            RuleFor(p => p.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
        }
    }
}

