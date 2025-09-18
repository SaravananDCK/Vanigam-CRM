using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class PaymentValidator : AbstractValidator<Payment>
    {
        public PaymentValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.InvoiceId).NotEmpty().WithMessage(localizer["InvoiceRequired"]);
            RuleFor(c => c.Amount).GreaterThan(0).WithMessage(localizer["AmountMustBeGreaterThanZero"]);
            RuleFor(c => c.PaidAt).NotEmpty().WithMessage(localizer["PaidAtRequired"]);
        }
    }
}