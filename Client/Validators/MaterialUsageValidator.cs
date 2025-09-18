using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class MaterialUsageValidator : AbstractValidator<MaterialUsage>
    {
        public MaterialUsageValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.JobId).NotEmpty().WithMessage(localizer["JobRequired"]);
            RuleFor(c => c.InventoryItemId).NotEmpty().WithMessage(localizer["InventoryItemRequired"]);
            RuleFor(c => c.Quantity).GreaterThan(0).WithMessage(localizer["QuantityMustBeGreaterThanZero"]);
        }
    }
}