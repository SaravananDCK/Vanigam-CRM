using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class ContractValidator : AbstractValidator<Contract>
    {
        public ContractValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.CustomerId)
                .NotEmpty()
                .WithMessage(localizer["CustomerRequired"]);

            RuleFor(c => c.Title)
                .NotEmpty()
                .WithMessage(localizer["TitleRequired"])
                .MaximumLength(200)
                .WithMessage(localizer["TitleMaxLength"]);

            RuleFor(c => c.StartDate)
                .NotNull()
                .WithMessage(localizer["StartDateRequired"]);

            RuleFor(c => c.EndDate)
                .NotNull()
                .WithMessage(localizer["EndDateRequired"])
                .GreaterThan(c => c.StartDate)
                .When(c => c.StartDate.HasValue && c.EndDate.HasValue)
                .WithMessage(localizer["EndDateMustBeAfterStartDate"]);

            RuleFor(c => c.PartsReplacementLimit)
                .GreaterThanOrEqualTo(0)
                .When(c => c.PartsReplacementLimit.HasValue)
                .WithMessage(localizer["PartsReplacementLimitMustBePositive"]);
        }
    }
}