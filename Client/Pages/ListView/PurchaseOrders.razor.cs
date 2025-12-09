using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Client.Services;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class PurchaseOrders
    {
        [Inject] private PdfApiService PdfApiService { get; set; }
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                args.OrderBy = !string.IsNullOrWhiteSpace(args.OrderBy) ? args.OrderBy : $"{nameof(PurchaseOrder.UpdatedAtUtc)} desc";
                var result = await PurchaseOrderApiService.Get(filter: GetFilterString(args), expand: GetExpandString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage() { Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = ex.Message });
            }
        }
        protected async Task PreviewPdf(PurchaseOrder order)
        {
            try
            {
                await PdfApiService.PreviewPurchaseOrderPdfAsync(order.Oid);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["ErrorPreviewingPdf"]
                });
            }
        }
        protected async Task DownloadPdf(PurchaseOrder order)
        {
            try
            {
                await PdfApiService.DownloadPurchaseOrderPdfAsync(order.Oid);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = Localizer["Error"],
                    Detail = Localizer["ErrorDownloadingPdf"]
                });
            }
        }
        protected override string GetFilterString(LoadDataArgs args)
        {
            return new ODataFilter<PurchaseOrder>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.Number, SearchString)
                .ContainsOr(u => u.Reference, SearchString)
                .ContainsOr(u => u.Notes, SearchString)
                .EndGroup()
                .Build();
        }

        protected override string GetExpandString(LoadDataArgs args)
        {
           return new ODataExpand<PurchaseOrder>()
                .Expand(f => f.Party, f => f.Party.Name)
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditPurchaseOrder>(Localizer["AddPurchaseOrder"], null, 100, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<PurchaseOrder> args)
        {
            await Open(args.Data);
        }

        private async Task Open(PurchaseOrder purchaseorder)
        {
            await DialogService.OpenDialogWithOutHeaderAsync<EditPurchaseOrder>(Localizer["EditPurchaseOrder"], new Dictionary<string, object> { { "Oid", purchaseorder.Oid } }, 100, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(PurchaseOrder purchaseorder)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await PurchaseOrderApiService.Delete(oid: purchaseorder.Oid);

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
