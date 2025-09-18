using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class OpportunityValidator : AbstractValidator<Opportunity>
    {
        public OpportunityValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.LeadId).NotEmpty().WithMessage(localizer["LeadRequired"]);
            RuleFor(c => c.Title).NotEmpty().WithMessage(localizer["TitleRequired"]);
            RuleFor(c => c.EstimatedValue).GreaterThanOrEqualTo(0).WithMessage(localizer["EstimatedValueMustBeNonNegative"]);
            RuleFor(c => c.ExpectedCloseDate).NotEmpty().WithMessage(localizer["ExpectedCloseDateRequired"]);
        }
    }
}