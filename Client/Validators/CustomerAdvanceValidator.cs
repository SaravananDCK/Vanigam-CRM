using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class CustomerAdvanceValidator : AbstractValidator<CustomerAdvance>
    {
        public CustomerAdvanceValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.PaymentId).NotEmpty().WithMessage(localizer["PaymentRequired"]);
            RuleFor(c => c.Amount).GreaterThan(0).WithMessage(localizer["AmountMustBeGreaterThanZero"]);
            RuleFor(c => c.BalanceAmount).GreaterThanOrEqualTo(0).WithMessage(localizer["BalanceAmountMustBeGreaterThanOrEqualToZero"]);
            RuleFor(c => c.AppliedDate).NotEmpty().WithMessage(localizer["AppliedDateRequired"]);
        }
    }
}
