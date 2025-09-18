using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class SlaValidator : AbstractValidator<Sla>
    {
        public SlaValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
            RuleFor(c => c.ResponseHours).GreaterThan(0).WithMessage(localizer["ResponseHoursMustBeGreaterThanZero"]);
            RuleFor(c => c.ResolutionHours).GreaterThan(0).WithMessage(localizer["ResolutionHoursMustBeGreaterThanZero"]);
        }
    }
}