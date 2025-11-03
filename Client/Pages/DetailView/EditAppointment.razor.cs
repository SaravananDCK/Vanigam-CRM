using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditAppointment
    {
        [Parameter] public Guid? JobId { get; set; }
        private int ReadOnlyTabIndex { get; set; } = 0;
        [Inject] private AppointmentApiService AppointmentApiService { get; set; }
        [Parameter] public bool IsEmbeddedModeActive { get; set; } = false;
        private int EditTabIndex { get; set; } = 0;
        protected override async Task OnInitializedAsync()
        {
            if (Oid == Guid.Empty)
            {
                CurrentObject = new();
                // Pre-set JobId if provided (for embedded mode)
                if (JobId.HasValue)
                {
                    CurrentObject.JobId = JobId.Value;
                }
                IsReadOnlyMode = false; // Create mode - always editable
            }
            else
            {
                CurrentObject = await AppointmentApiService.GetByOid(oid: Oid, expand: "Job");
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
                    CurrentObject = await AppointmentApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await AppointmentApiService.Update(oid: Oid, CurrentObject);
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
