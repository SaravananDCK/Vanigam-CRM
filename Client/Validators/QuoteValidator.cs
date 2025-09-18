using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class QuoteValidator : AbstractValidator<Quote>
    {
        public QuoteValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Title).NotEmpty().WithMessage(localizer["TitleRequired"]);
            RuleFor(c => c.TotalAmount).GreaterThanOrEqualTo(0).WithMessage(localizer["TotalAmountMustBeNonNegative"]);
        }
    }
}