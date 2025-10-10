using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
            RuleFor(c => c.UnitPrice).GreaterThanOrEqualTo(0).WithMessage(localizer["PriceMustBeNonNegative"]);
            RuleFor(c => c.Cost).GreaterThanOrEqualTo(0).When(c => c.Cost.HasValue).WithMessage(localizer["CostMustBeNonNegative"]);
            RuleFor(c => c.WarrantyPeriodMonths).GreaterThanOrEqualTo(0).When(c => c.WarrantyPeriodMonths.HasValue).WithMessage(localizer["WarrantyPeriodMustBeNonNegative"]);
            RuleFor(c => c.GuaranteePeriodMonths).GreaterThanOrEqualTo(0).When(c => c.GuaranteePeriodMonths.HasValue).WithMessage(localizer["GuaranteePeriodMustBeNonNegative"]);
            RuleFor(c => c.AmcAnnualRate).GreaterThanOrEqualTo(0).When(c => c.AmcAnnualRate.HasValue).WithMessage(localizer["AmcRateMustBeNonNegative"]);
        }
    }
}
