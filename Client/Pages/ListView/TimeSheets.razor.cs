using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class TimeSheets
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await TimeSheetApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<TimeSheet>()
                .FilterByAnd(args.Filter);

            // Add job filter if in embedded mode by job
            if (IsEmbeddedMode && JobId.HasValue)
            {
                filter = filter.FilterByAnd(ts => ts.JobId == JobId.Value);
            }

            // Add technician filter if in embedded mode by technician
            if (IsEmbeddedMode && TechnicianId.HasValue)
            {
                filter = filter.FilterByAnd(ts => ts.TechnicianId == TechnicianId.Value);
            }

            // No additional searchable string properties for TimeSheet
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

            // If in embedded mode, pre-set the parent entity ID
            if (IsEmbeddedMode && JobId.HasValue)
            {
                parameters.Add("JobId", JobId.Value);
            }
            if (IsEmbeddedMode && TechnicianId.HasValue)
            {
                parameters.Add("TechnicianId", TechnicianId.Value);
            }

            await DialogService.OpenDialogAsync<EditTimeSheet>(Localizer["AddTimeSheet"], parameters.Any() ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<TimeSheet> args)
        {
            await Open(args.Data);
        }

        private async Task Open(TimeSheet timesheet)
        {
            await DialogService.OpenDialogAsync<EditTimeSheet>(Localizer["EditTimeSheet"], new Dictionary<string, object> { { "Oid", timesheet.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(TimeSheet timesheet)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await TimeSheetApiService.Delete(oid:timesheet.Oid);

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