using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class PaymentAllocations
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await PaymentAllocationApiService.Get(filter: GetFilterString(args), expand: "Payment,Invoice", orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            return new ODataFilter<PaymentAllocation>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.Notes, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditPaymentAllocation>(Localizer["AddPaymentAllocation"], null, 30, 50);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<PaymentAllocation> args)
        {
            await Open(args.Data);
        }

        private async Task Open(PaymentAllocation paymentallocation)
        {
            await DialogService.OpenDialogAsync<EditPaymentAllocation>(Localizer["EditPaymentAllocation"], new Dictionary<string, object> { { "Oid", paymentallocation.Oid } }, 30, 50);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(PaymentAllocation paymentallocation)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await PaymentAllocationApiService.Delete(oid:paymentallocation.Oid);

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
