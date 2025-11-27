using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Client.Components.Dialogs;
using Vanigam.CRM.Client.Services;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditLead
    {
        [Inject] private LeadApiService LeadApiService { get; set; }
        bool IsFullheightTab = false;
        // Property to determine if the lead can be converted to opportunity
        private bool CanConvertToOpportunity => CurrentObject != null &&
            (CurrentObject.Status == LeadStatus.Qualified || CurrentObject.Status == LeadStatus.Contacted) &&
            CurrentObject.Status != LeadStatus.Converted;
        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await LeadApiService.GetByOid(oid: Oid);
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
                    CurrentObject = await LeadApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await LeadApiService.Update(oid: Oid, CurrentObject);
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
        void OnTabChanged(int index)
        {
            var tabNames = new[] { "Activities" };
            var currentTab = GetTabName(index);

            IsFullheightTab = tabNames.Contains(currentTab);
        }

        string GetTabName(int index)
        {
            return index switch
            {
                0 => Localizer["ContactInformation"],
                1 => Localizer["AddressInformation"],
                2 => Localizer["BusinessContext"],
                3 => Localizer["Comments"],
                4 => Localizer["Activities"],
                _ => ""
            };
        }
        private async Task ShowConvertToOpportunityDialog()
        {
            if (CurrentObject == null) return;

            var result = await DialogService.OpenDialogAsync<ConvertLeadToOpportunityDialog>(
                Localizer["ConvertToOpportunity"],
                new Dictionary<string, object>
                {
                    { "Lead", CurrentObject }
                },
                50, 50);

            if (result != null)
            {
                // Refresh the current object to show updated status
                CurrentObject = await LeadApiService.GetByOid(oid: Oid);
                StateHasChanged();
                DialogService.CloseDialog(CurrentObject);
            }
        }
        private async Task Changed(LeadStatus status)
        {
            
            CurrentObject.Status = status;
            EditContext.NotifyFieldChanged(EditContext.Field(nameof(CurrentObject.Status)));
            StateHasChanged();
        }
    }
}
