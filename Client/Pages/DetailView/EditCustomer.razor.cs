using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditCustomer
    {
        [Inject] private CustomerApiService CustomerApiService { get; set; }
        bool IsFullheightTab = false;
        private int EditTabIndex { get; set; } = 0;
        private int ReadOnlyTabIndex { get; set; } = 0;
        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await CustomerApiService.GetByOid(oid: Oid);
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
                    CurrentObject = await CustomerApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await CustomerApiService.Update(oid: Oid, CurrentObject);
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
        void OnTabChanged(int index)
        {
            var tabNames = new[] { "Contacts", "Jobs", "FileDocuments" };
            var currentTab = GetTabName(index);

            IsFullheightTab = tabNames.Contains(currentTab);
        }

        string GetTabName(int index)
        {
            return index switch
            {
                0 => "Basic Info",
                1 => "Contact Info",
                2 => "Address",
                3 => "Business Information",
                4 => "Contacts",
                5 => "Jobs",
                6 => "FileDocuments",
                _ => ""
            };
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

        protected async Task ReloadButtonClick()
        {
            // Reload the current object from the API
            if (Oid != Guid.Empty)
            {
                CurrentObject = await CustomerApiService.GetByOid(oid: Oid);
                await InitEditContext();
                HasChanges = false;
                CanEdit = true;
                StateHasChanged();
            }
        }
    }
}
