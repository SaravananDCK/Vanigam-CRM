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
        [Inject] private InvoiceItemApiService InvoiceItemApiService { get; set; }

        private List<InvoiceItemDTO> invoiceItems = new();

        private bool HasAnyChanges => HasChanges || (invoiceItems?.Any(i => i.IsNew || i.IsDeleted) ?? false);

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

            await InitEditContext();
        }
        protected string GetExpandString()
        {
            return new ODataExpand<Invoice>()
                .Expand(f => f.Party, f => f.Party.Name)
                .Build();
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

            //if (Oid == Guid.Empty)
            //{
            //    invoiceItems = new List<InvoiceItemDTO>();
            //    return;
            //}

            //try
            //{
            //    var filter = $"Oid eq {Oid} and IsNotDeleted eq true";
            //    var result = await InvoiceItemApiService.Get(filter: filter, orderBy: "CreatedAtUtc", top: 100);

            //    invoiceItems = result.Value.Select(item => new InvoiceItemDTO
            //    {
            //        Oid = item.Oid,
            //        InventoryItemId = item.Item.Oid,
            //        InventoryItemName = item.Item?.Name ?? "-",
            //        Quantity = item.Quantity,
            //        UnitPrice = item.UnitPrice,
            //        DiscountAmount = item.DiscountAmount,
            //        TaxAmount = item.TaxAmount
            //    }).ToList();
            //}
            //catch (Exception ex)
            //{
            //    Logger.LogError(ex.Message, ex);
            //    NotificationService.Notify(new NotificationMessage
            //    {
            //        Severity = NotificationSeverity.Error,
            //        Summary = Localizer["Error"],
            //        Detail = ex.Message
            //    });
            //}
        }

        private void OnInvoiceItemsChanged(List<InvoiceItemDTO> updatedItems)
        {
            invoiceItems = updatedItems;
            CalculateTotalAmount();
        }

        private void OnTotalAmountChanged(decimal totalAmount)
        {
            CurrentObject.TotalAmount = totalAmount;
            StateHasChanged();
        }

        private void CalculateTotalAmount()
        {
            var subTotal = invoiceItems.Where(i => !i.IsDeleted).Sum(i => i.Total);
            var totalDiscount = invoiceItems.Where(i => !i.IsDeleted).Sum(i => i.DiscountAmount);
            var totalTax = invoiceItems.Where(i => !i.IsDeleted).Sum(i => i.TaxAmount ?? 0);

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
        private async Task SaveBulkInvoice()
        {
            if (CurrentObject == null) return;

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
