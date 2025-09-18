using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class CustomFields
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await CustomFieldApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            return new ODataFilter<CustomField>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.EntityName, SearchString)
                .ContainsOr(u => u.FieldName, SearchString)
                .ContainsOr(u => u.FieldType, SearchString)
                .ContainsOr(u => u.MetadataJson, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditCustomField>(Localizer["AddCustomField"], null, 30, 50);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<CustomField> args)
        {
            await Open(args.Data);
        }

        private async Task Open(CustomField customfield)
        {
            await DialogService.OpenDialogAsync<EditCustomField>(Localizer["EditCustomField"], new Dictionary<string, object> { { "Oid", customfield.Oid } }, 30, 50);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(CustomField customfield)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await CustomFieldApiService.Delete(oid:customfield.Oid);

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