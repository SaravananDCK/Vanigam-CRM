using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators;

public class ServiceItemValidator : AbstractValidator<ServiceItem>
{
    public ServiceItemValidator(IStringLocalizer localizer)
    {
        RuleFor(c => c.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
    }
}