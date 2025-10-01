using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class StockLedgerEntryValidator : AbstractValidator<StockLedgerEntry>
    {
        public StockLedgerEntryValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.EntryNumber).NotEmpty().WithMessage(localizer["EntryNumberRequired"]);
            RuleFor(c => c.InventoryItemId).NotEmpty().WithMessage(localizer["InventoryItemRequired"]);
        }
    }
}
