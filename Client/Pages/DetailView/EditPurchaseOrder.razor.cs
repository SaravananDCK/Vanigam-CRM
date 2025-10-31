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
        [Inject] private VendorApiService VendorApiService { get; set; }
        private List<PurchaseOrderItemDTO> purchaseItems = new();
        private bool hasQuoteItemChanges = false;
        public bool HasAnyChanges => Form?.EditContext?.IsModified() == true || hasQuoteItemChanges;
        private IEnumerable<Vendor> Vendors { get; set; } = [];
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            // Load dropdown data
            await LoadDropdownData();

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

            //await LoadVendors();
            await InitEditContext();
        }
        protected string GetExpandString()
        {
            return new ODataExpand<PurchaseOrder>()
                .Expand(f => f.VoucherLines)
                .Expand(f => f.Party, f => f.Party.Name)
                .Build();
        }
        private async Task LoadDropdownData()
        {
            try
            {
                var vendorsTask = VendorApiService.Get();
                Vendors = (await vendorsTask).Value;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["Failed to load Dropdown Data"]
                });
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
            purchaseItems = items;
            hasQuoteItemChanges = true;
            StateHasChanged();
        }

        private async Task OnTotalAmountChanged(decimal totalAmount)
        {
            if (CurrentObject != null)
            {
                CurrentObject.TotalAmount = totalAmount;
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
                    DiscountAmount = CurrentObject.DiscountAmount,
                    DiscountPercentage = CurrentObject.DiscountPercent,
                    Items = purchaseItems,
                    DueDate = CurrentObject.DueDate,
                    ExpectedDeliveryDate = CurrentObject.ExpectedDeliveryDate,
                    ShippingAddress = CurrentObject.ShippingAddress,
                    ContactPerson = CurrentObject.ContactPerson,
                    Reference = CurrentObject.Reference
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
