using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Jobs
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                // In embedded mode, avoid navigation property sorting that can cause server errors
                var orderBy = args.OrderBy;
                if (IsEmbeddedMode && !string.IsNullOrEmpty(orderBy) && orderBy.Contains("Customer"))
                {
                    orderBy = "Title"; // Default to sorting by Title in embedded mode
                }

                var result = await JobApiService.Get(filter: GetFilterString(args), orderBy: orderBy, top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<Job>()
                .FilterByAnd(args.Filter);

            // Add customer filter if in embedded mode
            if (IsEmbeddedMode && CustomerId.HasValue)
            {
                filter = filter.FilterByAnd(j => j.CustomerId == CustomerId.Value);
            }

            // Add search filter only if there's a search string
            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = filter.BeginGroup()
                    .ContainsOr(u => u.Title, SearchString)
                    .ContainsOr(u => u.Description, SearchString)
                    .EndGroup();
            }

            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();

            // If in embedded mode with a customer, pre-set the CustomerId
            if (IsEmbeddedMode && CustomerId.HasValue)
            {
                parameters.Add("CustomerId", CustomerId.Value);
            }

            await DialogService.OpenDialogAsync<EditJob>(Localizer["AddJob"], parameters.Any() ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Job> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Job job)
        {
            await DialogService.OpenDialogAsync<EditJob>(Localizer["EditJob"], new Dictionary<string, object> { { "Oid", job.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Job job)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await JobApiService.Delete(oid:job.Oid);

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