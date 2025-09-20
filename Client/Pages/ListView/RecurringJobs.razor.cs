using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class RecurringJobs
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await RecurringJobApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<RecurringJob>()
                .FilterByAnd(args.Filter);

            // Add contract filter if in embedded mode
            if (IsEmbeddedMode && ContractId.HasValue)
            {
                filter = filter.FilterByAnd(rj => rj.ContractId == ContractId.Value);
            }

            // Add search filter only if there's a search string
            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = filter.BeginGroup()
                    .ContainsOr(u => u.Name, SearchString)
                    .ContainsOr(u => u.CronExpression, SearchString)
                    .EndGroup();
            }

            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();

            // If in embedded mode with a contract, pre-set the ContractId
            if (IsEmbeddedMode && ContractId.HasValue)
            {
                parameters.Add("ContractId", ContractId.Value);
            }

            await DialogService.OpenDialogAsync<EditRecurringJob>(Localizer["AddRecurringJob"], parameters.Any() ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<RecurringJob> args)
        {
            await Open(args.Data);
        }

        private async Task Open(RecurringJob recurringjob)
        {
            await DialogService.OpenDialogAsync<EditRecurringJob>(Localizer["EditRecurringJob"], new Dictionary<string, object> { { "Oid", recurringjob.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(RecurringJob recurringjob)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await RecurringJobApiService.Delete(oid:recurringjob.Oid);

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