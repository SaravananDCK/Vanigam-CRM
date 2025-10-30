using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditPurchaseInvoice
    {
        [Inject] private PurchaseInvoiceApiService PurchaseInvoiceApiService { get; set; }
        private List<PurchaseInvoiceItemDTO> PurchaseInvoiceItems = new();
        private bool hasQuoteItemChanges = false;
        public bool HasAnyChanges => Form?.EditContext?.IsModified() == true || hasQuoteItemChanges;

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await PurchaseInvoiceApiService.GetByOid(oid: Oid, expand: GetExpandString());
                IsReadOnlyMode = true; // Edit mode - start in read-only
                await LoadInvoiceItems();
            }

            await InitEditContext();
        }
        protected string GetExpandString()
        {
            return new ODataExpand<PurchaseInvoice>()
                .Expand(f => f.VoucherLines)
                .Expand(f => f.Party, f => f.Party.Name)
                .Build();
        }
        private async Task SaveBulkInvoice()
        {
            if (CurrentObject == null) return;

            var isValid = EditContext.Validate();
            if (!isValid) return;

            // Check if invoice is not posted and show confirmation dialog
            if (CurrentObject.Status != PurchaseInvoiceStatus.Posted)
            {
                var confirmResult = await DialogService.Confirm(
                    Localizer["DoYouWantToPostThisPurchaseInvoice"],
                    Localizer["PostInvoice"],
                    new ConfirmOptions() { OkButtonText = Localizer["Yes"], CancelButtonText = Localizer["No"] }
                );

                if (confirmResult == true)
                {
                    // User chose to post the invoice
                    CurrentObject.Status = PurchaseInvoiceStatus.Posted;
                }
            }
            IsBusy = true;
            try
            {
                // Prepare bulk save DTO with invoice and items
                var bulkData = new PurchaseInvoiceBulkSaveDTO
                {
                    Oid = IsCreateMode ? null : CurrentObject.Oid,
                    Number = CurrentObject.Number,
                    Status = CurrentObject.Status,
                    PartyId = CurrentObject.PartyId,
                    TotalAmount = CurrentObject.TotalAmount,
                    SubTotal = CurrentObject.SubTotal,
                    TaxAmount = CurrentObject.TaxAmount,
                    DiscountAmount = CurrentObject.DiscountAmount,
                    DiscountPercentage = CurrentObject.DiscountPercent,
                    DueDate = CurrentObject.DueDate,
                    ReceivedDate = CurrentObject.ReceivedDate,
                    Items = PurchaseInvoiceItems.Select(i => new PurchaseInvoiceItemDTO
                    {
                        Oid = i.Oid,
                        InventoryItemId = i.InventoryItemId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = i.DiscountAmount,
                        TaxAmount = i.TaxAmount,
                        IsDeleted = i.IsDeleted
                    }).ToList()
                };

                var savedPurchaseInvoice = await PurchaseInvoiceApiService.BulkSaveInvoiceWithItemsAsync(bulkData);

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

        private async Task LoadInvoiceItems()
        {
            try
            {
                if (Oid != Guid.Empty)
                {
                    PurchaseInvoiceItems = await PurchaseInvoiceApiService.GetInvoiceItemsForEditingAsync(Oid);
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

        protected override async Task EnableReadOnlyModeAsync()
        {
            await base.EnableReadOnlyModeAsync();
            if (Oid != Guid.Empty)
            {
                await LoadInvoiceItems();
            }
        }

        private void OnInvoiceItemsChanged(List<PurchaseInvoiceItemDTO> updatedItems)
        {
            PurchaseInvoiceItems = updatedItems;
            CalculateTotalAmount();
        }

        private void OnTotalAmountChanged(decimal totalAmount)
        {
            CurrentObject.TotalAmount = totalAmount;
            StateHasChanged();
        }

        private void CalculateTotalAmount()
        {
            var subTotal = PurchaseInvoiceItems.Where(i => !i.IsDeleted).Sum(i => i.Total);
            var totalDiscount = PurchaseInvoiceItems.Where(i => !i.IsDeleted).Sum(i => i.DiscountAmount);
            var totalTax = PurchaseInvoiceItems.Where(i => !i.IsDeleted).Sum(i => i.TaxAmount ?? 0);

            CurrentObject.SubTotal = subTotal;
            CurrentObject.DiscountAmount = totalDiscount;
            CurrentObject.TaxAmount = totalTax;
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


        //[Inject] private PurchaseInvoiceApiService PurchaseInvoiceApiService { get; set; }
        //[Inject] private VendorApiService VendorApiService { get; set; }
        //[Inject] private PurchaseOrderApiService PurchaseOrderApiService { get; set; }

        //private IEnumerable<Vendor> Vendors { get; set; } = [];
        //private IEnumerable<PurchaseOrder> PurchaseOrders { get; set; } = [];

        //protected override async Task OnInitializedAsync()
        //{
        //    if (Oid == Guid.Empty)
        //        CurrentObject = new() { VoucherType = Objects.Entities.VoucherType.PurchaseInvoice, VoucherDate = DateTimeOffset.UtcNow };
        //    else
        //        CurrentObject = await PurchaseInvoiceApiService.GetByOid(oid: Oid);

        //    await LoadVendors();
        //    await LoadPurchaseOrders();
        //    await InitEditContext();
        //}

        //private async Task LoadVendors()
        //{
        //    try
        //    {
        //        var result = await VendorApiService.Get(filter: null, expand: null, orderBy: "Name", top: null, skip: null, count: false);
        //        Vendors = result.Value.AsODataEnumerable();
        //    }
        //    catch (Exception ex)
        //    {
        //        NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = Localizer["Error"], Detail = Localizer["LoadVendorsFailed"] });
        //    }
        //}

        //private async Task LoadPurchaseOrders()
        //{
        //    try
        //    {
        //        var result = await PurchaseOrderApiService.Get(filter: null, expand: null, orderBy: "Number desc", top: null, skip: null, count: false);
        //        PurchaseOrders = result.Value.AsODataEnumerable();
        //    }
        //    catch (Exception ex)
        //    {
        //        NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = Localizer["Error"], Detail = Localizer["LoadPurchaseOrdersFailed"] });
        //    }
        //}

        //protected async Task FormSubmit()
        //{
        //    IsBusy = true;
        //    try
        //    {
        //        if (Oid == Guid.Empty)
        //        {
        //            CurrentObject = await PurchaseInvoiceApiService.Create(CurrentObject);
        //        }
        //        else
        //        {
        //            var result = await PurchaseInvoiceApiService.Update(oid: Oid, CurrentObject);
        //            if (result.IsPreconditionFailed())
        //            {
        //                HasChanges = true;
        //                CanEdit = false;
        //                return;
        //            }
        //        }
        //        NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = Localizer["SavedSuccessfully!"] });
        //        DialogService.CloseDialog(CurrentObject);
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        if (ex.StatusCode == HttpStatusCode.Conflict)
        //        {
        //            ShowNotUniqueAlert = true;
        //        }
        //        else
        //        {
        //            ErrorVisible = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorVisible = true;
        //    }
        //    IsBusy = false;
        //}
    }
}
