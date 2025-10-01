using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class LedgerEntries
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await LedgerEntryApiService.Get(filter: GetFilterString(args), expand: GetExpandString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage() { Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = Localizer[$"Load"] });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            return new ODataFilter<LedgerEntry>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.EntryNumber, SearchString)
                .ContainsOr(u => u.Description, SearchString)
                .ContainsOr(u => u.Reference, SearchString)
                .EndGroup()
                .Build();
        }

        protected override string GetExpandString(LoadDataArgs args)
        {
            return "Account";
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditLedgerEntry>(Localizer["AddLedgerEntry"], null, 80, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<LedgerEntry> args)
        {
            await Open(args.Data);
        }

        private async Task Open(LedgerEntry ledgerentry)
        {
            await DialogService.OpenDialogWithOutHeaderAsync<EditLedgerEntry>(Localizer["EditLedgerEntry"], new Dictionary<string, object> { { "Oid", ledgerentry.Oid } }, 80, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(LedgerEntry ledgerentry)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await LedgerEntryApiService.Delete(oid: ledgerentry.Oid);

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
