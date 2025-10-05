using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class PaymentValidator : AbstractValidator<Payment>
    {
        public PaymentValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.PartyId).NotEmpty().WithMessage(localizer["CustomerRequired"]);
            RuleFor(c => c.AllocatedAmount).GreaterThan(0).WithMessage(localizer["AllocatedAmountMustBeGreaterThanZero"]);
            RuleFor(c => c.PaidAt).NotEmpty().WithMessage(localizer["PaidAtRequired"]);
        }
    }
}