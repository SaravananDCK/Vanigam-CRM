using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Vehicles
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await VehicleApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            return new ODataFilter<Vehicle>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.RegistrationNumber, SearchString)
                .ContainsOr(u => u.Make, SearchString)
                .ContainsOr(u => u.Model, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditVehicle>(Localizer["AddVehicle"], null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Vehicle> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Vehicle vehicle)
        {
            await DialogService.OpenDialogAsync<EditVehicle>(Localizer["EditVehicle"], new Dictionary<string, object> { { "Oid", vehicle.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Vehicle vehicle)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await VehicleApiService.Delete(oid:vehicle.Oid);

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