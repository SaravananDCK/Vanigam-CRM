using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class PurchaseOrderValidator : AbstractValidator<PurchaseOrder>
    {
        public PurchaseOrderValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.PartyId).NotEmpty().WithMessage(localizer["VendorRequired"]);
            RuleFor(c => c.TotalAmount).GreaterThanOrEqualTo(0).WithMessage(localizer["TotalAmountMustBeNonNegative"]);
        }
    }
}
