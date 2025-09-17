using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.OData;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class DocxMacroTemplates
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await DocxMacroTemplateApiService.Get(filter: GetFilterString(args), expand: GetExpandString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count: args.Top != null && args.Skip != null);
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
            await DialogService.OpenDialogAsync<EditDocxMacroTemplate>(@Localizer["AddHeading"], null, 50, 50);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<DocxMacroTemplate> args)
        {
            await Open(args.Data);
        }
        private async Task Open(DocxMacroTemplate docxMacroTemplate)
        {
            await DialogService.OpenDialogAsync<EditDocxMacroTemplate>(@Localizer["EditHeading"], new Dictionary<string, object> { { "Oid", docxMacroTemplate.Oid } });
            await GridReload();
        }
        protected async Task GridDeleteButtonClick(DocxMacroTemplate docxMacroTemplate)
        {
            try
            {
                if (await DialogService.Confirm(@Localizer["DeleteConfirmation"]) == true)
                {
                    var deleteResult = await DocxMacroTemplateApiService.Delete(oid: docxMacroTemplate.Oid);

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

