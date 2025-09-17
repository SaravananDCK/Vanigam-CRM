using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects;

namespace Vanigam.CRM.Client.Validators
{
    public class ApplicationTenantValidator : AbstractValidator<ApplicationTenant>
    {
        public ApplicationTenantValidator(IStringLocalizer localizer)
        {
            RuleFor(p => p.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
            RuleFor(p => p.Hosts).NotEmpty().WithMessage(localizer["HostsRequired"]);
        }
    }
}

