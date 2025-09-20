using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Contracts
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await ContractApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<Contract>()
                .FilterByAnd(args.Filter);

            // Filter by parent Customer if in embedded mode
            if (IsEmbeddedMode && CustomerId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.CustomerId == CustomerId.Value);
            }

            filter.BeginGroup()
                .ContainsOr(u => u.Title, SearchString)
                .ContainsOr(u => u.Terms, SearchString)
                .EndGroup();

            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();
            if (IsEmbeddedMode && CustomerId.HasValue)
            {
                parameters.Add("CustomerId", CustomerId.Value);
            }

            await DialogService.OpenDialogAsync<EditContract>(Localizer["AddContract"], parameters.Count > 0 ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Contract> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Contract contract)
        {
            await DialogService.OpenDialogAsync<EditContract>(Localizer["EditContract"], new Dictionary<string, object> { { "Oid", contract.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Contract contract)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await ContractApiService.Delete(oid:contract.Oid);

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