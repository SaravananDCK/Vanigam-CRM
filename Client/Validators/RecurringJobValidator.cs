using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class RecurringJobValidator : AbstractValidator<RecurringJob>
    {
        public RecurringJobValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
            RuleFor(c => c.ContractId).NotEmpty().WithMessage(localizer["ContractRequired"]);
            RuleFor(c => c.CronExpression).NotEmpty().WithMessage(localizer["CronExpressionRequired"]);
        }
    }
}