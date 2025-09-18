using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class ContractValidator : AbstractValidator<Contract>
    {
        public ContractValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Title).NotEmpty().WithMessage(localizer["TitleRequired"]);
            RuleFor(c => c.CustomerId).NotEmpty().WithMessage(localizer["CustomerRequired"]);
            RuleFor(c => c.StartDate).NotEmpty().WithMessage(localizer["StartDateRequired"]);
            RuleFor(c => c.EndDate).GreaterThan(c => c.StartDate).When(c => c.EndDate.HasValue)
                .WithMessage(localizer["EndDateMustBeAfterStartDate"]);
        }
    }
}