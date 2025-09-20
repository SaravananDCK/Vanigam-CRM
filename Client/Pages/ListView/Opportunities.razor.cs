using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Opportunities
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await OpportunityApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<Opportunity>()
                .FilterByAnd(args.Filter);

            // Filter by parent Lead if in embedded mode
            if (IsEmbeddedMode && LeadId.HasValue)
            {
                filter = filter.FilterByAnd(u => u.LeadId == LeadId.Value);
            }

            filter.BeginGroup()
                .ContainsOr(u => u.Title, SearchString)
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

            await DialogService.OpenDialogAsync<EditOpportunity>(Localizer["AddOpportunity"], parameters.Count > 0 ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Opportunity> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Opportunity opportunity)
        {
            await DialogService.OpenDialogAsync<EditOpportunity>(Localizer["EditOpportunity"], new Dictionary<string, object> { { "Oid", opportunity.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Opportunity opportunity)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await OpportunityApiService.Delete(oid:opportunity.Oid);

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