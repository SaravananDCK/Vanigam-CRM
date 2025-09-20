using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Technicians
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await TechnicianApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            return new ODataFilter<Technician>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.UserName, SearchString)
                .ContainsOr(u => u.Email, SearchString)
                .ContainsOr(u => u.Name, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditTechnician>(Localizer["AddTechnician"], null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Technician> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Technician technician)
        {
            await DialogService.OpenDialogAsync<EditTechnician>(Localizer["EditTechnician"], new Dictionary<string, object> { { "Oid", technician.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Technician technician)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await TechnicianApiService.Delete(oid:technician.Oid);

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