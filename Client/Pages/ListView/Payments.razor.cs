using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Payments
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await PaymentApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<Payment>()
                .FilterByAnd(args.Filter);

            // Filter by parent Invoice if in embedded mode
            if (IsEmbeddedMode && InvoiceId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.InvoiceId == InvoiceId.Value);
            }

            filter.BeginGroup()
                .ContainsOr(u => u.ProviderReference, SearchString)
                .EndGroup();

            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();
            if (IsEmbeddedMode && InvoiceId.HasValue)
            {
                parameters.Add("InvoiceId", InvoiceId.Value);
            }

            await DialogService.OpenDialogAsync<EditPayment>(Localizer["AddPayment"], parameters.Count > 0 ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Payment> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Payment payment)
        {
            await DialogService.OpenDialogAsync<EditPayment>(Localizer["EditPayment"], new Dictionary<string, object> { { "Oid", payment.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Payment payment)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await PaymentApiService.Delete(oid:payment.Oid);

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