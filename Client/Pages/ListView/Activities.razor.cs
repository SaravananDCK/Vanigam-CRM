using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Activities
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await ActivityApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<Activity>()
                .FilterByAnd(args.Filter);

            // Filter by parent Lead or Opportunity if in embedded mode
            if (IsEmbeddedMode && LeadId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.LeadId == LeadId.Value);
            }
            else if (IsEmbeddedMode && OpportunityId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.OpportunityId == OpportunityId.Value);
            }

            filter.BeginGroup()
                .ContainsOr(u => u.Type, SearchString)
                .ContainsOr(u => u.Notes, SearchString)
                .EndGroup();

            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();
            if (IsEmbeddedMode && LeadId.HasValue)
            {
                parameters.Add("LeadId", LeadId.Value);
            }
            else if (IsEmbeddedMode && OpportunityId.HasValue)
            {
                parameters.Add("OpportunityId", OpportunityId.Value);
            }

            await DialogService.OpenDialogAsync<EditActivity>(Localizer["AddActivity"], parameters.Count > 0 ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Activity> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Activity activity)
        {
            await DialogService.OpenDialogAsync<EditActivity>(Localizer["EditActivity"], new Dictionary<string, object> { { "Oid", activity.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Activity activity)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await ActivityApiService.Delete(oid:activity.Oid);

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