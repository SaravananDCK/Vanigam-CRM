using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Quotes
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await QuoteApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<Quote>()
                .FilterByAnd(args.Filter);

            // Filter by parent Job if in embedded mode
            if (IsEmbeddedMode && JobId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.JobId == JobId.Value);
            }

            filter.BeginGroup()
                .ContainsOr(u => u.Title, SearchString)
                .EndGroup();

            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();
            if (IsEmbeddedMode && JobId.HasValue)
            {
                parameters.Add("JobId", JobId.Value);
            }

            await DialogService.OpenDialogAsync<EditQuote>(Localizer["AddQuote"], parameters.Count > 0 ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Quote> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Quote quote)
        {
            await DialogService.OpenDialogAsync<EditQuote>(Localizer["EditQuote"], new Dictionary<string, object> { { "Oid", quote.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Quote quote)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await QuoteApiService.Delete(oid:quote.Oid);

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