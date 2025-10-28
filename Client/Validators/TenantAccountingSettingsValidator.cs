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

        // Company information validation
        RuleFor(s => s.CompanyName)
            .MaximumLength(200)
            .WithMessage(localizer["CompanyNameMaxLength200"]);

        RuleFor(s => s.CompanyAddress)
            .MaximumLength(500)
            .WithMessage(localizer["CompanyAddressMaxLength500"]);

        RuleFor(s => s.CompanyCity)
            .MaximumLength(100)
            .WithMessage(localizer["CompanyCityMaxLength100"]);

        RuleFor(s => s.CompanyState)
            .MaximumLength(100)
            .WithMessage(localizer["CompanyStateMaxLength100"]);

        RuleFor(s => s.CompanyPostalCode)
            .MaximumLength(20)
            .WithMessage(localizer["CompanyPostalCodeMaxLength20"]);

        RuleFor(s => s.CompanyCountry)
            .MaximumLength(100)
            .WithMessage(localizer["CompanyCountryMaxLength100"]);

        RuleFor(s => s.CompanyPhone)
            .MaximumLength(50)
            .WithMessage(localizer["CompanyPhoneMaxLength50"]);

        RuleFor(s => s.CompanyEmail)
            .MaximumLength(100)
            .EmailAddress()
            .When(s => !string.IsNullOrEmpty(s.CompanyEmail))
            .WithMessage(localizer["ValidEmailRequired"]);

        RuleFor(s => s.CompanyWebsite)
            .MaximumLength(200)
            .WithMessage(localizer["CompanyWebsiteMaxLength200"]);

        RuleFor(s => s.CompanyTaxId)
            .MaximumLength(50)
            .WithMessage(localizer["CompanyTaxIdMaxLength50"]);

        RuleFor(s => s.CompanyRegistrationNumber)
            .MaximumLength(50)
            .WithMessage(localizer["CompanyRegistrationNumberMaxLength50"]);
    }
}
