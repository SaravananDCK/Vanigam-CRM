using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class ItemCategories
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await ItemCategoryApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            return new ODataFilter<ItemCategory>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.Description, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditItemCategory>(Localizer["AddItemCategory"], null, 30, 50);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<ItemCategory> args)
        {
            await Open(args.Data);
        }

        private async Task Open(ItemCategory itemcategory)
        {
            await DialogService.OpenDialogAsync<EditItemCategory>(Localizer["EditItemCategory"], new Dictionary<string, object> { { "Oid", itemcategory.Oid } }, 30, 50);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(ItemCategory itemcategory)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await ItemCategoryApiService.Delete(oid:itemcategory.Oid);

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
