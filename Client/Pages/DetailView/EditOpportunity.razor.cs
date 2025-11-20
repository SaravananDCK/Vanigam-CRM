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
    public partial class EditOpportunity
    {
        [Inject] private OpportunityApiService OpportunityApiService { get; set; }
        [Inject] private LeadApiService ConversionApiService { get; set; }
        bool IsFullheightTab = false;
        // Property to track selected information tab in read-only mode
        private int SelectedInfoTabIndex = 0;

        // Property to determine if the opportunity can be converted to customer
        private bool CanConvertToCustomer => CurrentObject != null &&
            (CurrentObject.Stage == OpportunityStage.Proposal ||
             CurrentObject.Stage == OpportunityStage.Negotiation ||
             CurrentObject.Stage == OpportunityStage.Qualified) &&
            CurrentObject.Stage != OpportunityStage.ClosedWon &&
            CurrentObject.Stage != OpportunityStage.ClosedLost;

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await OpportunityApiService.GetByOid(expand: "Lead", oid: Oid);
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
                    CurrentObject = await OpportunityApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await OpportunityApiService.Update(oid: Oid, CurrentObject);
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
        void OnTabChanged(int index)
        {
            var tabNames = new[] { "Activities"};
            var currentTab = GetTabName(index);

            IsFullheightTab = tabNames.Contains(currentTab);
        }

        string GetTabName(int index)
        {
            return index switch
            {
                0 => "OpportunityInformation",
                1 => "LeadInformation",
                2 => "Activities",
                _ => ""
            };
        }
        private async Task ShowConvertToCustomerDialog()
        {
            if (CurrentObject == null) return;

            var result = await DialogService.OpenDialogAsync<ConvertOpportunityToCustomerDialog>(
                Localizer["ConvertToCustomer"],
                new Dictionary<string, object>
                {
                    { "Opportunity", CurrentObject }
                },
                50, 50);

            if (result != null)
            {
                // Refresh the current object to show updated status
                CurrentObject = await OpportunityApiService.GetByOid(expand: "Lead", oid: Oid);
                StateHasChanged();
                DialogService.CloseDialog();
            }
        }

    }
}
