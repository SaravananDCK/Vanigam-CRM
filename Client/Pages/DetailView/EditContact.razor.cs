using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditContact
    {
        [Inject] private ContactApiService ContactApiService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // Load customers for dropdown
            var customerResult = await CustomerApiService.Get(top: 1000); // Load all customers
            Customers = customerResult.Value?.AsODataEnumerable();

            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                // If CustomerId is provided via parameter, set it
                if (CustomerId.HasValue)
                {
                    CurrentObject.CustomerId = CustomerId.Value;
                }
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await ContactApiService.GetByOid(oid: Oid);
                IsReadOnlyMode = true; // Edit mode - start in read-only
            }

            await InitEditContext();
        }
        
        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await ContactApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await ContactApiService.Update(oid: Oid, CurrentObject);
                    if(result.IsPreconditionFailed())
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
