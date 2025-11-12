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
    public partial class EditPurchaseInvoice
    {
        private int ReadOnlyTabIndex { get; set; } = 0;
        private int EditTabIndex { get; set; } = 0;
        [Inject] private PurchaseInvoiceApiService PurchaseInvoiceApiService { get; set; }
        [Inject] private TenantAccountingSettingsApiService TenantAccountingSettingsApiService { get; set; }
        private IEnumerable<Vendor> Vendors { get; set; } = [];
        private List<PurchaseInvoiceItemDTO> purchaseInvoiceItems = new();
        public string TenantAccountingState { get; set; }
        private string VendorState { get; set; }
        private bool HasAnyChanges => HasChanges || (purchaseInvoiceItems?.Any(i => i.IsNew || i.IsDeleted) ?? false) || Form?.EditContext?.IsModified() == true;

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new() { VoucherType = VoucherType.PurchaseInvoice, VoucherDate = DateTimeOffset.UtcNow };
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await PurchaseInvoiceApiService.GetByOid(oid: Oid, expand: GetExpandString());
                IsReadOnlyMode = true; // Edit mode - start in read-only
                await LoadPurchaseInvoiceItems();
            }
            var result = await TenantAccountingSettingsApiService.Get(top: 1);
            var accSetings = result?.Value?.FirstOrDefault(f => !string.IsNullOrEmpty(f.CompanyState));
            TenantAccountingState = accSetings?.CompanyState;
            await InitEditContext();
        }

        protected string GetExpandString()
        {
            return new ODataExpand<PurchaseInvoice>()
                .Expand(f => f.VoucherLines)
                .Expand(f => f.Party, f => f.Party.Name)
                .Expand(f => f.PurchaseOrder, f => f.PurchaseOrder.Number)
                .Build();
        }
       
        private async Task LoadPurchaseInvoiceItems()
        {
            try
            {
                if (Oid != Guid.Empty)
                {
                    purchaseInvoiceItems = await PurchaseInvoiceApiService.GetPurchaseInvoiceItemsForEditingAsync(Oid);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["FailedToLoadPurchaseInvoiceItems"]
                });
            }
        }

        private void OnPurchaseInvoiceItemsChanged(List<PurchaseInvoiceItemDTO> updatedItems)
        {
            purchaseInvoiceItems = updatedItems;
            VendorState = Vendors.Where(v => v.Oid == CurrentObject.PartyId).Select(v => v.State).FirstOrDefault();
            CalculateTotalAmount();
        }

        private void CalculateTotalAmount()
        {
            var subTotal = purchaseInvoiceItems.Where(i => !i.IsDeleted).Sum(i => i.Total);
            var totalDiscount = purchaseInvoiceItems.Where(i => !i.IsDeleted).Sum(i => i.DiscountAmount);
            var totalTax = purchaseInvoiceItems.Where(i => !i.IsDeleted).Sum(i => i.TaxAmount ?? 0);

            // Calculate GST breakdown from purchase invoice items
            decimal cgstAmount = 0;
            decimal sgstAmount = 0;
            decimal igstAmount = 0;
            decimal cessAmount = 0;

            foreach (var item in purchaseInvoiceItems.Where(i => !i.IsDeleted))
            {
                // Calculate taxable amount for this line (after discount)
                var taxableAmount = item.Total - item.DiscountAmount;

                // Calculate GST components based on rates from TaxCode
                if (TenantAccountingState == VendorState)
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
                EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.DiscountType)));
                StateHasChanged();
            }
        }

        private async Task OnDiscountAmountChanged(decimal discountAmount)
        {
            if (CurrentObject != null)
            {
                CurrentObject.DiscountAmount = discountAmount;
                EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.DiscountAmount)));
                StateHasChanged();
            }
        }

        private async Task OnDiscountPercentageChanged(decimal discountPercent)
        {
            if (CurrentObject != null)
            {
                CurrentObject.DiscountPercent = discountPercent;
                EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.DiscountPercent)));
                if (CurrentObject.DiscountPercent > 0)
                {
                    await OnDiscountAmountChanged(CurrentObject.DiscountAmount);
                }
                StateHasChanged();
            }
        }

        private async Task SaveBulkPurchaseInvoice()
        {
            if (CurrentObject == null) return;

            var isValid = EditContext.Validate();
            if (!isValid) return;

            // Check if purchase invoice is not posted and show confirmation dialog
            if (CurrentObject.Status != PurchaseInvoiceStatus.Posted)
            {
                var confirmResult = await DialogService.Confirm(
                    Localizer["DoYouWantToPostThisPurchaseInvoice"],
                    Localizer["PostPurchaseInvoice"],
                    new ConfirmOptions() { OkButtonText = Localizer["Yes"], CancelButtonText = Localizer["No"] }
                );

                if (confirmResult == true)
                {
                    // User chose to post the purchase invoice
                    CurrentObject.Status = PurchaseInvoiceStatus.Posted;
                }
            }

            IsBusy = true;
            try
            {
                // Prepare bulk save DTO with purchase invoice and items
                var bulkData = new PurchaseInvoiceBulkSaveDTO
                {
                    Oid = IsCreateMode ? null : CurrentObject.Oid,
                    Number = CurrentObject.Number,
                    Status = CurrentObject.Status,
                    PartyId = CurrentObject.PartyId,
                    VendorInvoiceNumber = CurrentObject.VendorInvoiceNumber,
                    ReceivedDate = CurrentObject.ReceivedDate,
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
                    DueDate = CurrentObject.DueDate,
                    VoucherDate = CurrentObject.VoucherDate,
                    PurchaseOrderId = CurrentObject.PurchaseOrderId,
                    Items = purchaseInvoiceItems.Select(i => new PurchaseInvoiceItemDTO
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

                var savedPurchaseInvoice = await PurchaseInvoiceApiService.BulkSavePurchaseInvoiceWithItemsAsync(bulkData);

                if (savedPurchaseInvoice != null)
                {
                    CurrentObject = savedPurchaseInvoice;
                    await EnableReadOnlyModeAsync();
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = Localizer["Success"],
                        Detail = Localizer["PurchaseInvoiceSavedSuccessfully"]
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
                await LoadPurchaseInvoiceItems();
            }
        }

        protected async Task FormSubmit()
        {
            if (purchaseInvoiceItems.Any() && purchaseInvoiceItems.FirstOrDefault().InventoryItemId != null)
            {
                await SaveBulkPurchaseInvoice();
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Failed"],
                    Detail = Localizer["At least one Purchase Invoice Item is required.."]
                });
            }
        }
    }
}
