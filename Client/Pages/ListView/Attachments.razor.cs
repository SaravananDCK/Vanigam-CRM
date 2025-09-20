using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Attachments
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await AttachmentApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<Attachment>()
                .FilterByAnd(args.Filter);

            // Filter by parent JobReport if in embedded mode
            if (IsEmbeddedMode && JobReportId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.JobReportId == JobReportId.Value);
            }

            filter.BeginGroup()
                .ContainsOr(u => u.FileName, SearchString)
                .ContainsOr(u => u.ContentType, SearchString)
                .ContainsOr(u => u.Url, SearchString)
                .EndGroup();

            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();
            if (IsEmbeddedMode && JobReportId.HasValue)
            {
                parameters.Add("JobReportId", JobReportId.Value);
            }

            await DialogService.OpenDialogAsync<EditAttachment>(Localizer["AddAttachment"], parameters.Count > 0 ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Attachment> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Attachment attachment)
        {
            await DialogService.OpenDialogAsync<EditAttachment>(Localizer["EditAttachment"], new Dictionary<string, object> { { "Oid", attachment.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Attachment attachment)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await AttachmentApiService.Delete(oid:attachment.Oid);

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