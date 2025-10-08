using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Products
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await ProductApiService.Get(filter: GetFilterString(args),expand:GetExpandString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                NotificationService.Notify(new NotificationMessage(){ Severity = NotificationSeverity.Error, Summary = Localizer[$"Error"], Detail = ex.Message });
            }
        }

        protected override string GetFilterString(LoadDataArgs args)
        {
            return new ODataFilter<Product>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.SKU, SearchString)
                .EndGroup()
                .Build();
        }
        protected override string GetExpandString(LoadDataArgs args)
        {
            return nameof(Item.TaxCode);
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditProduct>(Localizer["Add Product"], null, 100, 100);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Product> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Product product)
        {
            await DialogService.OpenDialogAsync<EditProduct>(Localizer["Edit Product"], new Dictionary<string, object> { { "Oid", product.Oid } }, 100, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Product product)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await ProductApiService.Delete(oid:product.Oid);

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