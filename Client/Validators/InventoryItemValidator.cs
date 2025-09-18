using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class InventoryItemValidator : AbstractValidator<InventoryItem>
    {
        public InventoryItemValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
            RuleFor(c => c.QuantityOnHand).GreaterThanOrEqualTo(0).WithMessage(localizer["QuantityMustBeNonNegative"]);
        }
    }
}