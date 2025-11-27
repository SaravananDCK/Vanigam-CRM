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
        [Inject] private PurchaseOrderApiService PurchaseOrderApiService { get; set; }
        [Inject] private TenantAccountingSettingsApiService TenantAccountingSettingsApiService { get; set; }
        [Inject] private VendorApiService VendorApiService { get; set; }
        private static readonly IList<PurchaseOrderStatus> PurchaseOrderStatuses = [.. Enum.GetValues<PurchaseOrderStatus>()];
        private IEnumerable<Vendor> Vendors { get; set; } = [];
        private List<PurchaseOrderItemDTO> purchaseItems = new();
        private bool hasItemChanges = false;
        public string TenantAccountingState { get; set; }
        private string VendorState { get; set; }
        public bool HasAnyChanges => Form?.EditContext?.IsModified() == true || hasItemChanges || (purchaseItems?.Any(i => i.IsNew || i.IsDeleted) ?? false);
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
            await LoadVendors();
            await InitEditContext();
        }
        private async Task OnChanged(PurchaseOrderStatus status)
        {
            CurrentObject.Status = status;
            EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.Status)));
            StateHasChanged();
        }
        protected string GetExpandString()
        {
            return new ODataExpand<PurchaseOrder>()
                .Expand(f => f.VoucherLines)
                .Expand(f => f.Party, f => f.Party.Name)
                .Build();
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
            VendorState = Vendors.Where(v => v.Oid == CurrentObject.PartyId).Select(v => v.State).FirstOrDefault();
            
            if (items.Any(i => i.InventoryItemId == null)) return;

            purchaseItems = items;
            hasItemChanges = true;
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
            CurrentObject.DiscountType = type;
            StateHasChanged();
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
       
        protected override async Task EnableReadOnlyModeAsync()
        {
            await base.EnableReadOnlyModeAsync();
            hasItemChanges = false;
            await LoadPurchaseItems(); // Reload original data
        }
        protected async Task FormSubmit()
        {
            if (purchaseItems.Any() && purchaseItems.FirstOrDefault().InventoryItemId != null)
            {
                await SaveBulkQuote();
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Failed"],
                    Detail = Localizer["At least one Purchase Order Item is required.."]
                });
            }
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
                    hasItemChanges = false;

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
