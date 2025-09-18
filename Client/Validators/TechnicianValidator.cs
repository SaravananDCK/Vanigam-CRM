using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class TechnicianValidator : AbstractValidator<Technician>
    {
        public TechnicianValidator(IStringLocalizer localizer)
        {
            // Technician inherits from ApplicationUser, so we might want to validate inherited properties
            // ApplicationUser likely has email, username etc. that should be validated
            RuleFor(c => c.Email).EmailAddress().When(c => !string.IsNullOrEmpty(c.Email))
                .WithMessage(localizer["EmailInvalid"]);
            RuleFor(c => c.UserName).NotEmpty().WithMessage(localizer["UserNameRequired"]);
        }
    }
}