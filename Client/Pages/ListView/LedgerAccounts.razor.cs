using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class LedgerAccounts
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await LedgerAccountApiService.Get(
                    filter: GetFilterString(args),
                    expand: "AccountGroup",
                    orderBy: $"{args.OrderBy}",
                    top: args.Top,
                    skip: args.Skip,
                    count: args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message,ex);
                NotificationService.Notify(new NotificationMessage() { Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = ex.Message });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            return new ODataFilter<LedgerAccount>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.Code, SearchString)
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.Email, SearchString)
                .ContainsOr(u => u.Phone, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditLedgerAccount>(Localizer["AddLedgerAccount"], null, 100, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<LedgerAccount> args)
        {
            await Open(args.Data);
        }

        private async Task Open(LedgerAccount ledgeraccount)
        {
            await DialogService.OpenDialogAsync<EditLedgerAccount>(Localizer["EditLedgerAccount"], new Dictionary<string, object> { { "Oid", ledgeraccount.Oid } }, 100, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(LedgerAccount ledgeraccount)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await LedgerAccountApiService.Delete(oid: ledgeraccount.Oid);

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
