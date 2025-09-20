using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class UserSessions
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await UserSessionApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            return new ODataFilter<UserSession>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.UserName, SearchString)
                .ContainsOr(u => u.DeviceInfo, SearchString)
                .ContainsOr(u => u.IpAddress, SearchString)
                .ContainsOr(u => u.UserAgent, SearchString)
                .ContainsOr(u => u.Location, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditUserSession>(Localizer["AddUserSession"], null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<UserSession> args)
        {
            await Open(args.Data);
        }

        private async Task Open(UserSession usersession)
        {
            await DialogService.OpenDialogAsync<EditUserSession>(Localizer["EditUserSession"], new Dictionary<string, object> { { "Oid", usersession.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(UserSession usersession)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await UserSessionApiService.Delete(oid:usersession.Oid);

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