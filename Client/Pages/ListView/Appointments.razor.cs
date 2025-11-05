using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Appointments
    {
        [Parameter] public Guid? JobId { get; set; }
        [Parameter] public bool IsEmbeddedMode { get; set; } = false;
        [Parameter] public string? EmbeddedTitle { get; set; }
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await AppointmentApiService.Get(filter: GetFilterString(args),expand:GetExpandString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage(){ Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = ex.Message });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            var filter = new ODataFilter<Appointment>()
                .FilterByAnd(args.Filter);

            // Add job filter if in embedded mode
            if (IsEmbeddedMode && JobId.HasValue)
            {
                filter = filter.FilterByAnd(a => a.JobId == JobId);
            }

            // No additional searchable string properties for Appointment
            // Only add search group if there's a search string
            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = filter.BeginGroup()
                    .EndGroup();
            }

            return filter.Build();
        }
        protected override string GetExpandString(LoadDataArgs args)
        {
            return new ODataExpand<Appointment>()
                .Expand(f => f.Job, f => f.Job.Title)
                .Expand(f => f.Technician, f => f.Technician.FullName)
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();

            // If in embedded mode with a job, pre-set the JobId
            if (IsEmbeddedMode && JobId.HasValue)
            {
                parameters.Add("JobId", JobId.Value);
                parameters.Add("IsEmbeddedModeActive", IsEmbeddedMode);
            }

            await DialogService.OpenDialogAsync<EditAppointment>(Localizer["Add Appointment"], parameters.Any() ? parameters : null, 100, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Appointment> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Appointment appointment)
        {
            await DialogService.OpenDialogAsync<EditAppointment>(Localizer["Edit Appointment"], new Dictionary<string, object> { { "Oid", appointment.Oid },{ "IsEmbeddedModeActive", IsEmbeddedMode } }, 100, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Appointment appointment)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await AppointmentApiService.Delete(oid:appointment.Oid);

                    if (deleteResult != null)
                    {
                        await GridReload();
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = Localizer[$"Success"],
                            Detail = Localizer[$"SuccessfullyDeleted"]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer[$"Error"],
                    Detail = Localizer[$"UnableDelete"]
                });
            }
        }
    }
}