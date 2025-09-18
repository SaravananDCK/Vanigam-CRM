using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class QuoteItemValidator : AbstractValidator<QuoteItem>
    {
        public QuoteItemValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.QuoteId).NotEmpty().WithMessage(localizer["QuoteRequired"]);
            RuleFor(c => c.Quantity).GreaterThan(0).WithMessage(localizer["QuantityMustBeGreaterThanZero"]);
            RuleFor(c => c.UnitPrice).GreaterThanOrEqualTo(0).WithMessage(localizer["UnitPriceMustBeNonNegative"]);
        }
    }
}