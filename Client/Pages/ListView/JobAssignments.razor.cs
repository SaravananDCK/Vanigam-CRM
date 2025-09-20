using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class JobAssignments
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await JobAssignmentApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage(){ Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = Localizer[$"Load"] });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            var filter = new ODataFilter<JobAssignment>()
                .FilterByAnd(args.Filter);

            // Add job filter if in embedded mode
            if (IsEmbeddedMode && JobId.HasValue)
            {
                filter = filter.FilterByAnd(ja => ja.JobId == JobId.Value);
            }

            // No additional searchable string properties for JobAssignment
            // Only add search group if there's a search string
            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = filter.BeginGroup()
                    .EndGroup();
            }

            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();

            // If in embedded mode with a job, pre-set the JobId
            if (IsEmbeddedMode && JobId.HasValue)
            {
                parameters.Add("JobId", JobId.Value);
            }

            await DialogService.OpenDialogAsync<EditJobAssignment>(Localizer["AddJobAssignment"], parameters.Any() ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<JobAssignment> args)
        {
            await Open(args.Data);
        }

        private async Task Open(JobAssignment jobassignment)
        {
            await DialogService.OpenDialogAsync<EditJobAssignment>(Localizer["EditJobAssignment"], new Dictionary<string, object> { { "Oid", jobassignment.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(JobAssignment jobassignment)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await JobAssignmentApiService.Delete(oid:jobassignment.Oid);

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