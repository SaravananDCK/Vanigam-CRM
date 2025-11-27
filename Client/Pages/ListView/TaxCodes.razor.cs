using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class TaxCodes
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await TaxCodeApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null);
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
            return new ODataFilter<TaxCode>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.Code, SearchString)
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.TaxType, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditTaxCode>(Localizer["AddTaxCode"], null, 100, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<TaxCode> args)
        {
            await Open(args.Data);
        }

        private async Task Open(TaxCode taxcode)
        {
            await DialogService.OpenDialogAsync<EditTaxCode>(Localizer["EditTaxCode"], new Dictionary<string, object> { { "Oid", taxcode.Oid } }, 100, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(TaxCode taxcode)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await TaxCodeApiService.Delete(oid: taxcode.Oid);

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
