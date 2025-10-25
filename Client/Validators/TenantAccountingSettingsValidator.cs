using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators;

public class TenantAccountingSettingsValidator : AbstractValidator<TenantAccountingSettings>
{
    public TenantAccountingSettingsValidator(IStringLocalizer localizer)
    {
        // Validate fiscal year start month
        RuleFor(s => s.FiscalYearStartMonth)
            .Must(m => string.IsNullOrEmpty(m) || (int.TryParse(m, out var month) && month >= 1 && month <= 12))
            .WithMessage(localizer["FiscalYearStartMonthMustBeBetween1And12"]);

        // Validate default currency
        RuleFor(s => s.DefaultCurrency)
            .MaximumLength(50)
            .WithMessage(localizer["DefaultCurrencyMaxLength50"]);

        // Conditional validation: If AutoPostInvoices is enabled, require sales account
        RuleFor(s => s.DefaultSalesAccountId)
            .NotEmpty()
            .When(s => s.AutoPostInvoices)
            .WithMessage(localizer["SalesAccountRequiredForAutoPosting"]);

        // Conditional validation: If AutoPostPurchaseInvoices is enabled, require purchases account
        RuleFor(s => s.DefaultPurchasesAccountId)
            .NotEmpty()
            .When(s => s.AutoPostPurchaseInvoices)
            .WithMessage(localizer["PurchasesAccountRequiredForAutoPosting"]);

        // Notes length validation
        RuleFor(s => s.Notes)
            .MaximumLength(500)
            .WithMessage(localizer["NotesMaxLength500"]);
    }
}
