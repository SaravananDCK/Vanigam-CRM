using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;
using Microsoft.AspNetCore.Components;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class FileDocuments
    {
        [Parameter] public Guid? LedgerAccountId { get; set; }
        [Parameter] public bool IsEmbeddedMode { get; set; } = false;
        [Parameter] public string? EmbeddedTitle { get; set; }
        [Parameter] public string? HeightVH { get; set; } 
        FileDocument CurrentFileDocument;
        IList<FileDocument> selectedFileDocuments;
        private DataGridSelectionMode GridSelectionMode { get; set; } = DataGridSelectionMode.Single;
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await FileDocumentApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<FileDocument>()
               .FilterByAnd(args.Filter);

            // Add customer filter if in embedded mode
            if (IsEmbeddedMode && LedgerAccountId.HasValue)
            {
                filter = filter.FilterByAnd(c => c.LedgerAccountId == LedgerAccountId.Value);
            }
            // Add search filter only if there's a search string
            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = filter.BeginGroup()
                    .ContainsOr(u => u.FileName, SearchString)
                    .EndGroup();
            }
            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<AddFileDocument>(Localizer["AddFileDocument"], new Dictionary<string, object> { { "LedgerAccountId", LedgerAccountId } }, 60, 50);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<FileDocument> args)
        {
            await Open(args.Data);
        }

        private async Task Open(FileDocument filedocument)
        {
            await DialogService.OpenDialogWithOutHeaderAsync<EditFileDocument>(Localizer["EditFileDocument"], new Dictionary<string, object> { { "Oid", filedocument.Oid } }, 75, 100);
            await GridReload();
        }
        protected async Task ShowPdfView(DataGridRowMouseEventArgs<FileDocument> args)
        {
            CurrentFileDocument = await FileDocumentApiService.GetFileContent(oid: args.Data.Oid);
        }
        private async Task SelectButtonClick(MouseEventArgs arg)
        {
            if (GridSelectionMode == DataGridSelectionMode.Single)
            {
                GridSelectionMode = DataGridSelectionMode.Multiple;
            }
            else if (GridSelectionMode == DataGridSelectionMode.Multiple)
            {
                GridSelectionMode = DataGridSelectionMode.Single;
                selectedFileDocuments = null;
                await GridReload();
            }
        }
        protected async Task GridDeleteButtonClick(FileDocument filedocument)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await FileDocumentApiService.Delete(oid:filedocument.Oid);

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