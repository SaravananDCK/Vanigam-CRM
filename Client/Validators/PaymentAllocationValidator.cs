using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class PaymentAllocationValidator : AbstractValidator<PaymentAllocation>
    {
        public PaymentAllocationValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.PaymentId).NotEmpty().WithMessage(localizer["PaymentRequired"]);
            RuleFor(c => c.InvoiceId).NotEmpty().WithMessage(localizer["InvoiceRequired"]);
            RuleFor(c => c.Amount).GreaterThan(0).WithMessage(localizer["AmountMustBeGreaterThanZero"]);
            RuleFor(c => c.AppliedDate).NotEmpty().WithMessage(localizer["AppliedDateRequired"]);
        }
    }
}
