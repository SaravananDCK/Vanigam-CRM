using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class MaterialUsages
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await MaterialUsageApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<MaterialUsage>()
                .FilterByAnd(args.Filter);

            // Add job filter if in embedded mode by job
            if (IsEmbeddedMode && JobId.HasValue)
            {
                filter = filter.FilterByAnd(mu => mu.JobId == JobId.Value);
            }

            // Add inventory item filter if in embedded mode by inventory item
            if (IsEmbeddedMode && InventoryItemId.HasValue)
            {
                filter = filter.FilterByAnd(mu => mu.InventoryItemId == InventoryItemId.Value);
            }

            // No additional searchable string properties for MaterialUsage
            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = filter.BeginGroup()
                    .EndGroup();
            }

            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();

            // If in embedded mode, pre-set the parent entity ID
            if (IsEmbeddedMode && JobId.HasValue)
            {
                parameters.Add("JobId", JobId.Value);
            }
            if (IsEmbeddedMode && InventoryItemId.HasValue)
            {
                parameters.Add("InventoryItemId", InventoryItemId.Value);
            }

            await DialogService.OpenDialogAsync<EditMaterialUsage>(Localizer["AddMaterialUsage"], parameters.Any() ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<MaterialUsage> args)
        {
            await Open(args.Data);
        }

        private async Task Open(MaterialUsage materialusage)
        {
            await DialogService.OpenDialogAsync<EditMaterialUsage>(Localizer["EditMaterialUsage"], new Dictionary<string, object> { { "Oid", materialusage.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(MaterialUsage materialusage)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await MaterialUsageApiService.Delete(oid:materialusage.Oid);

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