using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using Vanigam.CRM.Client.Pages.ListView;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditInvoice
    {
        [Inject] private InvoiceApiService InvoiceApiService { get; set; }
        [Inject] private TenantAccountingSettingsApiService TenantAccountingSettingsApiService { get; set; }
        [Inject] private CustomerApiService CustomerApiService { get; set; }
        private IEnumerable<Customer> Customers { get; set; } = [];
        private List<InvoiceItemDTO> invoiceItems = new();
        public string TenantAccountingState { get; set; }
        private string CurrentState { get; set; }
        private bool HasAnyChanges => HasChanges || (invoiceItems?.Any(i => i.IsNew || i.IsDeleted) ?? false || Form?.EditContext?.IsModified() == true);
        private static readonly IList<InvoiceStatus> InvoiceStatuses = [.. Enum.GetValues<InvoiceStatus>()];
        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await InvoiceApiService.GetByOid(oid: Oid, expand: GetExpandString());
                IsReadOnlyMode = true; // Edit mode - start in read-only
                await LoadInvoiceItems();
            }
            var result = await TenantAccountingSettingsApiService.Get(top: 1);
            var accSetings = result?.Value?.FirstOrDefault(f => !string.IsNullOrEmpty(f.CompanyState));
            TenantAccountingState = accSetings?.CompanyState;
            await LoadCustomers();
            await InitEditContext();
        }
        protected string GetExpandString()
        {
            return new ODataExpand<Invoice>()
                .Expand(f => f.VoucherLines)
                .Expand(f => f.Party, f => f.Party.Name)
                .Expand(f => f.Quote, f => f.Quote.Number)
                .Build();
        }
        private async Task Changed(InvoiceStatus status)
        {
            CurrentObject.Status = status;
            EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.Status)));
            StateHasChanged();
        }
        private async Task LoadCustomers()
        {
            try
            {
                var result = await CustomerApiService.Get(filter: null, expand: null, orderBy: "Name", top: null, skip: null, count: false);
                Customers = result.Value.AsODataEnumerable();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = Localizer["Error"], Detail = Localizer["LoadVendorsFailed"] });
            }
        }
        private async Task LoadInvoiceItems()
        {
            try
            {
                if (Oid != Guid.Empty)
                {
                    invoiceItems = await InvoiceApiService.GetInvoiceItemsForEditingAsync(Oid);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["FailedToLoadInvoiceItems"]
                });
            }
        }

        private void OnInvoiceItemsChanged(List<InvoiceItemDTO> updatedItems)
        {
            CurrentState = Customers.Where(v => v.Oid == CurrentObject.PartyId).Select(v => v.State).FirstOrDefault();

            if (updatedItems.Any(i => i.InventoryItemId == null)) return;
            invoiceItems = updatedItems;
            CalculateTotalAmount();
        }
        private void CalculateTotalAmount()
        {
            var subTotal = invoiceItems.Where(i => !i.IsDeleted).Sum(i => i.Total);
            var totalDiscount = invoiceItems.Where(i => !i.IsDeleted).Sum(i => i.DiscountAmount);
            var totalTax = invoiceItems.Where(i => !i.IsDeleted).Sum(i => i.TaxAmount ?? 0);

            // Calculate GST breakdown from invoice items
            decimal cgstAmount = 0;
            decimal sgstAmount = 0;
            decimal igstAmount = 0;
            decimal cessAmount = 0;

            foreach (var item in invoiceItems.Where(i => !i.IsDeleted))
            {
                // Calculate taxable amount for this line (after discount)
                var taxableAmount = item.Total - item.DiscountAmount;

                // Calculate GST components based on rates from TaxCode
                if (TenantAccountingState == CurrentState)
                {
                    cgstAmount += taxableAmount * (decimal)(item.CGSTRate / 100);
                    sgstAmount += taxableAmount * (decimal)(item.SGSTRate / 100);
                }
                else
                {
                    igstAmount += taxableAmount * (decimal)(item.IGSTRate / 100);
                }
                cessAmount += taxableAmount * (decimal)(item.CessRate / 100);
            }

            CurrentObject.SubTotal = Math.Round(subTotal);
            CurrentObject.DiscountAmount = totalDiscount;
            CurrentObject.TaxAmount = Math.Round(totalTax);
            CurrentObject.CGSTAmount = cgstAmount;
            CurrentObject.SGSTAmount = sgstAmount;
            CurrentObject.IGSTAmount = igstAmount;
            CurrentObject.CessAmount = cessAmount;
            CurrentObject.TotalAmount = Math.Round(subTotal - totalDiscount + totalTax);
            StateHasChanged();
        }

        private async Task OnDiscountTypeChanged(DiscountType type)
        {
            if (CurrentObject != null)
            {
                CurrentObject.DiscountType = type;
                StateHasChanged();
            }
        }

        private async Task OnDiscountPercentageChanged(decimal discountPercent)
        {
            if (CurrentObject != null && discountPercent != 0)
            {
                CurrentObject.DiscountPercent = discountPercent;
                EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.DiscountPercent)));
                StateHasChanged();
            }
        }

        protected async Task FormSubmit()
        {
            if (invoiceItems.Any() && invoiceItems.FirstOrDefault().InventoryItemId != null)
            {
                await SaveBulkInvoice();
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Failed"],
                    Detail = Localizer["At least one Invoice Item is required.."]
                });
            }
        }

        private async Task SaveBulkInvoice()
        {
            if (CurrentObject == null) return;

            var isValid = EditContext.Validate();
            if (!isValid) return;

            // Check if invoice is not posted and show confirmation dialog
            if (CurrentObject.Status != InvoiceStatus.Posted)
            {
                var confirmResult = await DialogService.Confirm(
                    Localizer["DoYouWantToPostThisInvoice"],
                    Localizer["PostInvoice"],
                    new ConfirmOptions() { OkButtonText = Localizer["Yes"], CancelButtonText = Localizer["No"] }
                );

                if (confirmResult == true)
                {
                    // User chose to post the invoice
                    CurrentObject.Status = InvoiceStatus.Posted;
                }
            }
            IsBusy = true;
            try
            {
                // Prepare bulk save DTO with invoice and items
                var bulkData = new InvoiceBulkSaveDTO
                {
                    Oid = IsCreateMode ? null : CurrentObject.Oid,
                    Number = CurrentObject.Number,
                    Status = CurrentObject.Status,
                    PartyId = CurrentObject.PartyId,
                    TotalAmount = CurrentObject.TotalAmount,
                    SubTotal = CurrentObject.SubTotal,
                    TaxAmount = CurrentObject.TaxAmount,
                    CGSTAmount = CurrentObject.CGSTAmount,
                    SGSTAmount = CurrentObject.SGSTAmount,
                    IGSTAmount = CurrentObject.IGSTAmount,
                    CessAmount = CurrentObject.CessAmount,
                    DiscountType = CurrentObject.DiscountType,
                    DiscountAmount = CurrentObject.DiscountAmount,
                    DiscountPercentage = CurrentObject.DiscountPercent,
                    Items = invoiceItems.Select(i => new InvoiceItemDTO
                    {
                        Oid = i.Oid,
                        InventoryItemId = i.InventoryItemId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = i.DiscountAmount,
                        TaxAmount = i.TaxAmount,
                        TaxCodeId = i.TaxCodeId,
                        CGSTRate = i.CGSTRate,
                        SGSTRate = i.SGSTRate,
                        IGSTRate = i.IGSTRate,
                        CessRate = i.CessRate,
                        IsDeleted = i.IsDeleted
                    }).ToList()
                };

                var savedInvoice = await InvoiceApiService.BulkSaveInvoiceWithItemsAsync(bulkData);

                if (savedInvoice != null)
                {
                    CurrentObject = savedInvoice;
                    await EnableReadOnlyModeAsync();
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = Localizer["Success"],
                        Detail = Localizer["InvoiceSavedSuccessfully"]
                    });
                    DialogService.CloseDialog(CurrentObject);
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    ShowNotUniqueAlert = true;
                }
                else
                {
                    ErrorVisible = true;
                }
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                ErrorVisible = true;
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = ex.Message
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected override async Task EnableReadOnlyModeAsync()
        {
            await base.EnableReadOnlyModeAsync();
            if (Oid != Guid.Empty)
            {
                await LoadInvoiceItems();
            }
        }
    }
}
