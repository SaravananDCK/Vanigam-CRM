using Vanigam.CRM.Objects.OData;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class DocxTemplates
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await DocxTemplateApiService.Get(filter: GetFilterString(args), expand: GetExpandString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null);
                DataSource = result.Value.AsODataEnumerable();
                Count = result.Count;

            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage() { Severity = NotificationSeverity.Error, Summary = @Localizer[$"Error"], Detail = @Localizer[$"Load"] });
            }
        }
        protected override string GetFilterString(LoadDataArgs args)
        {
            return new ODataFilter<DocxTemplate>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.Content, SearchString)
                .ContainsOr(u => u.FileName, SearchString)
                .ContainsOr(u => u.Expands, SearchString)
                .EndGroup()
                .Build();
        }
        protected override string GetExpandString(LoadDataArgs args)
        {
            return new ODataExpand<DocxTemplate>()
                .Expand(f => f.FileCategory, f => f.FileCategory.Name)
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditDocxTemplate>(@Localizer["AddHeading"], null, 50, 50);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<DocxTemplate> args)
        {
            await Open(args.Data);
        }
        private async Task Open(DocxTemplate docxTemplate)
        {
            await DialogService.OpenDialogAsync<EditDocxTemplate>(@Localizer["EditHeading"], new Dictionary<string, object> { { "Oid", docxTemplate.Oid } });
            await GridReload();
        }
        protected async Task GridDeleteButtonClick(DocxTemplate docxTemplate)
        {
            try
            {
                if (await DialogService.Confirm(@Localizer["DeleteConfirmation"]) == true)
                {
                    var deleteResult = await DocxTemplateApiService.Delete(oid: docxTemplate.Oid);

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
                    Summary = @Localizer[$"Error"],
                    Detail = @Localizer[$"DeleteNotification"]
                });
            }
        }
    }
}

