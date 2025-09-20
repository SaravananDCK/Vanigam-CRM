using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Leads
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await LeadApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            return new ODataFilter<Lead>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.Organization, SearchString)
                .ContainsOr(u => u.Email, SearchString)
                .ContainsOr(u => u.Phone, SearchString)
                .ContainsOr(u => u.JobTitle, SearchString)
                .ContainsOr(u => u.Industry, SearchString)
                .ContainsOr(u => u.Source, SearchString)
                .ContainsOr(u => u.CampaignSource, SearchString)
                .ContainsOr(u => u.City, SearchString)
                .ContainsOr(u => u.State, SearchString)
                .ContainsOr(u => u.ProductOfInterest, SearchString)
                .ContainsOr(u => u.Comments, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditLead>(Localizer["AddLead"], null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Lead> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Lead lead)
        {
            await DialogService.OpenDialogAsync<EditLead>(Localizer["EditLead"], new Dictionary<string, object> { { "Oid", lead.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Lead lead)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await LeadApiService.Delete(oid:lead.Oid);

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