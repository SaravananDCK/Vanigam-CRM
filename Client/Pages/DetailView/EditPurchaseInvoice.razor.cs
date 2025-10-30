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
        [Inject] private PurchaseInvoiceApiService PurchaseInvoiceApiService { get; set; }
        [Inject] private PurchaseInvoiceItemApiService PurchaseInvoiceItemApiService { get; set; }
        [Inject] private VendorApiService VendorApiService { get; set; }
        [Inject] private PurchaseOrderApiService PurchaseOrderApiService { get; set; }

        private IEnumerable<Vendor> Vendors { get; set; } = [];
        private IEnumerable<PurchaseOrder> PurchaseOrders { get; set; } = [];

        private List<PurchaseInvoiceItemDTO> purchaseInvoiceItems = new();

        private bool HasAnyChanges => HasChanges || (purchaseInvoiceItems?.Any(i => i.IsNew || i.IsDeleted) ?? false);

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new() { VoucherType = Objects.Entities.VoucherType.PurchaseInvoice, VoucherDate = DateTimeOffset.UtcNow };
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await PurchaseInvoiceApiService.GetByOid(oid: Oid, expand: GetExpandString());
                IsReadOnlyMode = true; // Edit mode - start in read-only
                await LoadPurchaseInvoiceItems();
            }

            await LoadVendors();
            await LoadPurchaseOrders();
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

        private async Task LoadVendors()
        {
            try
            {
                var result = await VendorApiService.Get(filter: null, expand: null, orderBy: "Name", top: null, skip: null, count: false);
                Vendors = result.Value.AsODataEnumerable();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = Localizer["Error"], Detail = Localizer["LoadVendorsFailed"] });
            }
        }

        private async Task LoadPurchaseOrders()
        {
            try
            {
                var result = await PurchaseOrderApiService.Get(filter: null, expand: null, orderBy: "Number desc", top: null, skip: null, count: false);
                PurchaseOrders = result.Value.AsODataEnumerable();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = Localizer["Error"], Detail = Localizer["LoadPurchaseOrdersFailed"] });
            }
        }

        private void OnPurchaseInvoiceItemsChanged(List<PurchaseInvoiceItemDTO> updatedItems)
        {
            purchaseInvoiceItems = updatedItems;
            CalculateTotalAmount();
        }

        private void OnTotalAmountChanged(decimal totalAmount)
        {
            CurrentObject.TotalAmount = totalAmount;
            StateHasChanged();
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
                cgstAmount += taxableAmount * (decimal)(item.CGSTRate / 100);
                sgstAmount += taxableAmount * (decimal)(item.SGSTRate / 100);
                igstAmount += taxableAmount * (decimal)(item.IGSTRate / 100);
                cessAmount += taxableAmount * (decimal)(item.CessRate / 100);
            }

            CurrentObject.SubTotal = subTotal;
            CurrentObject.DiscountAmount = totalDiscount;
            CurrentObject.TaxAmount = totalTax;
            CurrentObject.CGSTAmount = cgstAmount;
            CurrentObject.SGSTAmount = sgstAmount;
            CurrentObject.IGSTAmount = igstAmount;
            CurrentObject.CessAmount = cessAmount;
            CurrentObject.TotalAmount = subTotal - totalDiscount + totalTax;
        }

        private async Task OnSubTotalChanged(decimal subTotal)
        {
            if (CurrentObject != null)
            {
                CurrentObject.SubTotal = subTotal;
                StateHasChanged();
            }
        }

        private async Task OnDiscountTypeChanged(DiscountType type)
        {
            if (CurrentObject != null)
            {
                CurrentObject.DiscountType = type;
                StateHasChanged();
            }
        }

        private async Task OnTotalTaxAmountChanged(decimal taxAmount)
        {
            if (CurrentObject != null)
            {
                CurrentObject.TaxAmount = taxAmount;
                StateHasChanged();
            }
        }

        private async Task OnDiscountAmountChanged(decimal discountAmount)
        {
            if (CurrentObject != null)
            {
                CurrentObject.DiscountAmount = discountAmount;
                StateHasChanged();
            }
        }

        private async Task OnDiscountPercentageChanged(double discountPercent)
        {
            if (CurrentObject != null)
            {
                CurrentObject.DiscountPercent = discountPercent;
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
                    DiscountAmount = CurrentObject.DiscountAmount,
                    DiscountPercentage = CurrentObject.DiscountPercent,
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
            await SaveBulkPurchaseInvoice();
        }
    }
}
