using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditPurchaseInvoice
    {
        [Inject] private PurchaseInvoiceApiService PurchaseInvoiceApiService { get; set; }
        [Inject] private VendorApiService VendorApiService { get; set; }
        [Inject] private PurchaseOrderApiService PurchaseOrderApiService { get; set; }

        private IEnumerable<Vendor> Vendors { get; set; } = [];
        private IEnumerable<PurchaseOrder> PurchaseOrders { get; set; } = [];

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
                CurrentObject = new() { VoucherType = Objects.Entities.VoucherType.PurchaseInvoice, VoucherDate = DateTimeOffset.UtcNow };
            else
                CurrentObject = await PurchaseInvoiceApiService.GetByOid(oid: Oid);

            await LoadVendors();
            await LoadPurchaseOrders();
            await InitEditContext();
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

        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await PurchaseInvoiceApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await PurchaseInvoiceApiService.Update(oid: Oid, CurrentObject);
                    if (result.IsPreconditionFailed())
                    {
                        HasChanges = true;
                        CanEdit = false;
                        return;
                    }
                }
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = Localizer["SavedSuccessfully!"] });
                DialogService.CloseDialog(CurrentObject);
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
            }
            catch (Exception ex)
            {
                ErrorVisible = true;
            }
            IsBusy = false;
        }
    }
}
