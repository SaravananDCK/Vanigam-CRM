using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net;
using Vanigam.CRM.Client.Pages.ListView;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditPurchaseOrder
    {
        private int ReadOnlyTabIndex { get; set; } = 0;
        private int EditTabIndex { get; set; } = 0;
        [Inject] private PurchaseOrderApiService PurchaseOrderApiService { get; set; }
        [Inject] private TenantAccountingSettingsApiService TenantAccountingSettingsApiService { get; set; }
        private IEnumerable<Vendor> Vendors { get; set; } = [];
        private List<PurchaseOrderItemDTO> purchaseItems = new();
        private bool hasQuoteItemChanges = false;
        public string TenantAccountingState { get; set; }
        private string VendorState { get; set; }
        public bool HasAnyChanges => Form?.EditContext?.IsModified() == true || hasQuoteItemChanges || (purchaseItems?.Any(i => i.IsNew || i.IsDeleted) ?? false);
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if (CurrentObject != null && !IsCreateMode)
            {
                await LoadPurchaseItems();
            }
            else if (IsCreateMode)
            {
                purchaseItems = new List<PurchaseOrderItemDTO>();
            }
        }
        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new() { VoucherType = VoucherType.PurchaseOrder, VoucherDate = DateTimeOffset.UtcNow };
                IsReadOnlyMode = false;
            }
            else
            {
                CurrentObject = await PurchaseOrderApiService.GetByOid(oid: Oid, expand: GetExpandString());
                IsReadOnlyMode = true;
            }
            var result = await TenantAccountingSettingsApiService.Get(top: 1);
            var accSetings = result?.Value?.FirstOrDefault(f => !string.IsNullOrEmpty(f.CompanyState));
            TenantAccountingState = accSetings?.CompanyState;

            await InitEditContext();
        }
        protected string GetExpandString()
        {
            return new ODataExpand<PurchaseOrder>()
                .Expand(f => f.VoucherLines)
                .Expand(f => f.Party, f => f.Party.Name)
                .Build();
        }
        private async Task LoadPurchaseItems()
        {
            try
            {
                if (Oid != Guid.Empty)
                {
                    purchaseItems = await PurchaseOrderApiService.GetPurchaseOrderItemsForEditingAsync(Oid);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["FailedToLoadQuoteItems"]
                });
            }
        }

        private async Task OnPurchaseOrderItemsChanged(List<PurchaseOrderItemDTO> items)
        {
            purchaseItems = items;
            hasQuoteItemChanges = true;
            VendorState = Vendors.Where(v => v.Oid == CurrentObject.PartyId).Select(v => v.State).FirstOrDefault();
            CalculateTotalAmount();
        }
        private void CalculateTotalAmount()
        {
            var subTotal = purchaseItems.Where(i => !i.IsDeleted).Sum(i => i.Total);
            var totalDiscount = purchaseItems.Where(i => !i.IsDeleted).Sum(i => i.DiscountAmount);
            var totalTax = purchaseItems.Where(i => !i.IsDeleted).Sum(i => i.TaxAmount ?? 0);

            // Calculate GST breakdown from purchase invoice items
            decimal cgstAmount = 0;
            decimal sgstAmount = 0;
            decimal igstAmount = 0;
            decimal cessAmount = 0;

            foreach (var item in purchaseItems.Where(i => !i.IsDeleted))
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
       
        protected override async Task EnableReadOnlyModeAsync()
        {
            await base.EnableReadOnlyModeAsync();
            hasQuoteItemChanges = false;
            await LoadPurchaseItems(); // Reload original data
        }

        private async Task SaveBulkQuote()
        {
            if (CurrentObject == null) return;

            var isValid = EditContext.Validate();
            if (!isValid) return;

            IsBusy = true;
            try
            {
                var bulkData = new PurchaseOrderBulkSaveDTO
                {
                    Oid = IsCreateMode ? null : CurrentObject.Oid,
                    Title = CurrentObject.Number,
                    Status = CurrentObject.Status,
                    VendorId = CurrentObject.PartyId,
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
                    ExpectedDeliveryDate = CurrentObject.ExpectedDeliveryDate,
                    ShippingAddress = CurrentObject.ShippingAddress,
                    ContactPerson = CurrentObject.ContactPerson,
                    Reference = CurrentObject.Reference,
                    Items = purchaseItems
                };

                var savedQuote = await PurchaseOrderApiService.BulkSavePurchaseOrderWithItemsAsync(bulkData);

                if (savedQuote != null)
                {
                    CurrentObject = savedQuote;
                    hasQuoteItemChanges = false;

                    await LoadPurchaseItems(); // Refresh items
                    await EnableReadOnlyModeAsync();

                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = Localizer["Success"],
                        Detail = Localizer["PurchaseOrderSavedSuccessfully"]
                    });
                    DialogService.CloseDialog(CurrentObject);
                }
            }
            catch (Exception ex)
            {
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
    }
}
