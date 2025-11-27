using Microsoft.AspNetCore.Components;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditVendor
    {
        [Inject] private VendorApiService VendorApiService { get; set; }
        private static readonly IList<CustomerType> CustomerTypes = [.. Enum.GetValues<CustomerType>()];
        private static readonly IList<CustomerStatus> CustomerStatuses = [.. Enum.GetValues<CustomerStatus>()];
        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new() { AccountType = Objects.Entities.AccountType.Vendor };
                IsReadOnlyMode = false;
            }
            else
            {
                CurrentObject = await VendorApiService.GetByOid(oid: Oid);
                IsReadOnlyMode = true;
            }

            await InitEditContext();
        }
        private async Task OnChanged(CustomerStatus status)
        {

            CurrentObject.Status = status;
            EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.Status)));
            StateHasChanged();
        }
        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await VendorApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await VendorApiService.Update(oid: Oid, CurrentObject);
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

        protected override async Task SaveAndStayInEdit()
        {
            await FormSubmit();
            // After successful save, switch back to read-only mode
            if (!ErrorVisible && !ShowNotUniqueAlert)
            {
                IsReadOnlyMode = true;
                StateHasChanged();
            }
        }
    }
}
