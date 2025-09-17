using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class LanguageValidator : AbstractValidator<Language>
    {
        public LanguageValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Code).NotEmpty().WithMessage(localizer["CodeRequired"]);
            RuleFor(c => c.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
        }
    }
}

