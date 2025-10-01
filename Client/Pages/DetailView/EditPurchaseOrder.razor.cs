using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditPurchaseOrder
    {
        [Inject] private PurchaseOrderApiService PurchaseOrderApiService { get; set; }
        [Inject] private VendorApiService VendorApiService { get; set; }

        private IEnumerable<Vendor> Vendors { get; set; } = [];

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
                CurrentObject = new() { VoucherType = Objects.Entities.VoucherType.PurchaseOrder, VoucherDate = DateTimeOffset.UtcNow };
            else
                CurrentObject = await PurchaseOrderApiService.GetByOid(oid: Oid);

            await LoadVendors();
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

        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await PurchaseOrderApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await PurchaseOrderApiService.Update(oid: Oid, CurrentObject);
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
