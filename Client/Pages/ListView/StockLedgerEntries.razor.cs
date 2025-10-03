using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class StockLedgerEntries
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await StockLedgerEntryApiService.Get(filter: GetFilterString(args), expand: GetExpandString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null);
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
            return new ODataFilter<StockLedgerEntry>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.EntryNumber, SearchString)
                .ContainsOr(u => u.Description, SearchString)
                .ContainsOr(u => u.Reference, SearchString)
                .ContainsOr(u => u.BatchNumber, SearchString)
                .EndGroup()
                .Build();
        }

        protected override string GetExpandString(LoadDataArgs args)
        {
            return "InventoryItem,Location";
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditStockLedgerEntry>(Localizer["AddStockLedgerEntry"], null, 80, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<StockLedgerEntry> args)
        {
            await Open(args.Data);
        }

        private async Task Open(StockLedgerEntry stockentry)
        {
            await DialogService.OpenDialogWithOutHeaderAsync<EditStockLedgerEntry>(Localizer["EditStockLedgerEntry"], new Dictionary<string, object> { { "Oid", stockentry.Oid } }, 80, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(StockLedgerEntry stockentry)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await StockLedgerEntryApiService.Delete(oid: stockentry.Oid);

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
