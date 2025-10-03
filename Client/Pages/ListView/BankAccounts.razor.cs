using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class BankAccounts
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await BankAccountApiService.Get(filter: GetFilterString(args), expand: GetExpandString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null);
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
            return new ODataFilter<BankAccount>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.Code, SearchString)
                .ContainsOr(u => u.BankName, SearchString)
                .ContainsOr(u => u.AccountNumber, SearchString)
                .EndGroup()
                .Build();
        }

        protected override string GetExpandString(LoadDataArgs args)
        {
            return string.Empty;
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditBankAccount>(Localizer["AddBankAccount"], null, 80, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<BankAccount> args)
        {
            await Open(args.Data);
        }

        private async Task Open(BankAccount bankaccount)
        {
            await DialogService.OpenDialogWithOutHeaderAsync<EditBankAccount>(Localizer["EditBankAccount"], new Dictionary<string, object> { { "Oid", bankaccount.Oid } }, 80, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(BankAccount bankaccount)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await BankAccountApiService.Delete(oid: bankaccount.Oid);

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
