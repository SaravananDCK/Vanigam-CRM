using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NodaTime;
using NodaTime.Extensions;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditActivity
    {
        [Inject] private ActivityApiService ActivityApiService { get; set; }
        private int ActivityFor { get; set; }
        [Parameter] public Guid? LeadId { get; set; }
        [Parameter] public Guid? OpportunityId { get; set; }
        private int EditTabIndex { get; set; } = 0;

        // Dropdown data for form fields
        private List<string> ActivityTypes = new()
        {
            "Call", "Email", "Meeting", "Task", "Note", "Conversion", "Follow-up"
        };

        private List<string> ActivityStatuses = new()
        {
            "Pending", "Completed", "Cancelled", "In Progress"
        };

        // Helper property to convert between NodaTime Instant and DateTime for UI binding
        private DateTime ActivityDateForUI
        {
            get
            {
                if (CurrentObject?.ActivityDate != null)
                {
                    return Instant.FromDateTimeOffset(CurrentObject.ActivityDate).ToDateTimeUtc();
                }
                return SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            }
            set
            {
                if (CurrentObject != null)
                {
                    CurrentObject.ActivityDate = Instant.FromDateTimeUtc(value).ToDateTimeOffset();
                }
            }
        }

        // Helper method to display activity date in read-only mode
        private string GetActivityDateDisplay(DateTimeOffset activityDate)
        {
            return Instant.FromDateTimeOffset(activityDate).ToDateTimeUtc().ToString("yyyy-MM-dd HH:mm") + " UTC";
        }

        protected string GetExpandString()
        {
            return new ODataExpand<Activity>()
                .Expand(f => f.Lead, f => f.Lead.Name)
                .Expand(f => f.Opportunity, f => f.Opportunity.Title)
                .Build();
        }

        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                IsReadOnlyMode = false; // Create mode - always editable

                // Set the parent Lead or Opportunity when creating a new activity
                if (LeadId.HasValue)
                {
                    CurrentObject.LeadId = LeadId.Value;
                }
                else if (OpportunityId.HasValue)
                {
                    CurrentObject.OpportunityId = OpportunityId.Value;
                }
            }
            else
            {
                CurrentObject = await ActivityApiService.GetByOid(oid: Oid, expand: GetExpandString());
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
                    CurrentObject = await ActivityApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await ActivityApiService.Update(oid: Oid, CurrentObject);
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
