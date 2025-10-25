using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView;

public partial class EditTenantAccountingSettings
{
    [Inject] private TenantAccountingSettingsApiService TenantAccountingSettingsApiService { get; set; }

    private int EditTabIndex { get; set; } = 0;
    private int ReadOnlyTabIndex { get; set; } = 0;

    // Helper property to convert fiscal month string to/from numeric
    private int FiscalMonthNumeric
    {
        get => int.TryParse(CurrentObject?.FiscalYearStartMonth, out var month) ? month : 1;
        set => CurrentObject.FiscalYearStartMonth = value.ToString("D2");
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Try to load existing settings for current tenant
            var settingsResult = await TenantAccountingSettingsApiService.Get(top: 1);
            if (settingsResult?.Value?.Any() == true)
            {
                // Settings exist - load the first one (should only be one per tenant)
                var existingSettings = settingsResult.Value.FirstOrDefault();
                if (existingSettings != null)
                {
                    Oid = existingSettings.Oid;
                    CurrentObject = await TenantAccountingSettingsApiService.GetByOid(oid: Oid,
                        expand: "DefaultSalesAccount,SalesReturnAccount,SalesDiscountAccount," +
                                "DefaultPurchasesAccount,PurchaseReturnAccount,PurchaseDiscountAccount," +
                                "DefaultTaxPayableAccount,DefaultSGSTPayableAccount,DefaultCGSTPayableAccount,DefaultIGSTPayableAccount," +
                                "DefaultTaxInputAccount,DefaultSGSTInputAccount,DefaultCGSTInputAccount,DefaultIGSTInputAccount," +
                                "DefaultCashAccount,DefaultBankAccount,DefaultCardAccount,DefaultUpiAccount," +
                                "DefaultReceivableAccount,DefaultPayableAccount," +
                                "DefaultInventoryAccount,WorkInProgressAccount,CostOfGoodsSoldAccount," +
                                "RoundingAccount,ExchangeGainLossAccount,FreightChargesAccount,PackingChargesAccount");
                }
            }
            else
            {
                // No settings exist - create new
                CurrentObject = new TenantAccountingSettings
                {
                    IsActive = true,
                    AutoPostInvoices = true,
                    AutoPostPayments = true,
                    AutoPostPurchaseInvoices = true,
                    UseJobCosting = true,
                    UseInventoryAccounting = true,
                    RequireBalancedEntries = true,
                    FiscalYearStartMonth = "04", // April (Indian fiscal year)
                    DefaultCurrency = "INR"
                };
                Oid = Guid.Empty; // Indicates new record
            }

            await InitEditContext();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing TenantAccountingSettings");
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = Localizer["Error"],
                Detail = ex.Message
            });
        }
    }

    protected async Task FormSubmit()
    {
        IsBusy = true;
        try
        {
            if (Oid == Guid.Empty)
            {
                // Create new settings
                CurrentObject = await TenantAccountingSettingsApiService.Create(CurrentObject);
                Oid = CurrentObject.Oid;
            }
            else
            {
                // Update existing settings
                var result = await TenantAccountingSettingsApiService.Update(oid: Oid, CurrentObject);
                if (result.IsPreconditionFailed())
                {
                    HasChanges = true;
                    CanEdit = false;
                    return;
                }
            }

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = Localizer["Success"],
                Detail = Localizer["SavedSuccessfully!"]
            });

            // Reload to get navigation properties
            CurrentObject = await TenantAccountingSettingsApiService.GetByOid(oid: Oid,
                expand: "DefaultSalesAccount,SalesReturnAccount,SalesDiscountAccount," +
                        "DefaultPurchasesAccount,PurchaseReturnAccount,PurchaseDiscountAccount," +
                        "DefaultTaxPayableAccount,DefaultSGSTPayableAccount,DefaultCGSTPayableAccount,DefaultIGSTPayableAccount," +
                        "DefaultTaxInputAccount,DefaultSGSTInputAccount,DefaultCGSTInputAccount,DefaultIGSTInputAccount," +
                        "DefaultCashAccount,DefaultBankAccount,DefaultCardAccount,DefaultUpiAccount," +
                        "DefaultReceivableAccount,DefaultPayableAccount," +
                        "DefaultInventoryAccount,WorkInProgressAccount,CostOfGoodsSoldAccount," +
                        "RoundingAccount,ExchangeGainLossAccount,FreightChargesAccount,PackingChargesAccount");

            await InitEditContext();
            EnableReadOnlyMode();
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "HTTP error saving TenantAccountingSettings");
            ErrorVisible = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving TenantAccountingSettings");
            ErrorVisible = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
