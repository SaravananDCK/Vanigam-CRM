using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class JobReports
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await JobReportApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<JobReport>()
                .FilterByAnd(args.Filter);

            // Add job filter if in embedded mode
            if (IsEmbeddedMode && JobId.HasValue)
            {
                filter = filter.FilterByAnd(jr => jr.JobId == JobId.Value);
            }

            // Add search filter only if there's a search string
            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = filter.BeginGroup()
                    .ContainsOr(u => u.Notes, SearchString)
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

            await DialogService.OpenDialogAsync<EditJobReport>(Localizer["AddJobReport"], parameters.Any() ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<JobReport> args)
        {
            await Open(args.Data);
        }

        private async Task Open(JobReport jobreport)
        {
            await DialogService.OpenDialogAsync<EditJobReport>(Localizer["EditJobReport"], new Dictionary<string, object> { { "Oid", jobreport.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(JobReport jobreport)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await JobReportApiService.Delete(oid:jobreport.Oid);

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