using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class NumberSeriesList
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await NumberSeriesApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage() { Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = ex.Message });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            return new ODataFilter<Objects.Entities.NumberSeries>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.EntityType, SearchString)
                .ContainsOr(u => u.Prefix, SearchString)
                .ContainsOr(u => u.Suffix, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditNumberSeries>(Localizer["AddNumberSeries"], null, 30, 50);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Objects.Entities.NumberSeries> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Objects.Entities.NumberSeries numberseries)
        {
            await DialogService.OpenDialogAsync<EditNumberSeries>(Localizer["EditNumberSeries"], new Dictionary<string, object> { { "Oid", numberseries.Oid } }, 30, 50);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Objects.Entities.NumberSeries numberseries)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await NumberSeriesApiService.Delete(oid: numberseries.Oid);

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
