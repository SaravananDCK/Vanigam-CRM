using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Languages
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await LanguageApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            return new ODataFilter<Language>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.Code, SearchString)
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.Description, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditLanguage>(Localizer["AddLanguage"], null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Language> args)
        {
          await Open(args.Data);
        }
        private async Task Open(Language language)
        {
            await DialogService.OpenDialogAsync<EditLanguage>(Localizer["EditLanguage"], new Dictionary<string, object> { { "Oid", language.Oid } }, 80, 80);
            await GridReload();
        }
        protected async Task GridDeleteButtonClick(Language language)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await LanguageApiService.Delete(oid:language.Oid);

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
