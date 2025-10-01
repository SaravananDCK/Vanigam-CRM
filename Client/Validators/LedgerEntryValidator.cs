using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class LedgerEntryValidator : AbstractValidator<LedgerEntry>
    {
        public LedgerEntryValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.EntryNumber).NotEmpty().WithMessage(localizer["EntryNumberRequired"]);
            RuleFor(c => c.Amount).GreaterThan(0).WithMessage(localizer["AmountMustBeGreaterThanZero"]);
            RuleFor(c => c.AccountId).NotEmpty().WithMessage(localizer["AccountRequired"]);
            RuleFor(c => c.EntryType).IsInEnum().WithMessage(localizer["EntryTypeRequired"]);
        }
    }
}
