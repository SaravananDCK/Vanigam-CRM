using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Contacts
    {
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await ContactApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            var filter = new ODataFilter<Contact>()
                .FilterByAnd(args.Filter);

            // Add customer filter if in embedded mode
            if (IsEmbeddedMode && CustomerId.HasValue)
            {
                filter = filter.FilterByAnd(c => c.CustomerId == CustomerId.Value);
            }

            // Add search filter only if there's a search string
            if (!string.IsNullOrEmpty(SearchString))
            {
                filter = filter.BeginGroup()
                    .ContainsOr(u => u.FirstName, SearchString)
                    .ContainsOr(u => u.LastName, SearchString)
                    .ContainsOr(u => u.Email, SearchString)
                    .ContainsOr(u => u.Phone, SearchString)
                    .EndGroup();
            }

            return filter.Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var parameters = new Dictionary<string, object>();

            // If in embedded mode with a customer, pre-set the CustomerId
            if (IsEmbeddedMode && CustomerId.HasValue)
            {
                parameters.Add("CustomerId", CustomerId.Value);
            }

            await DialogService.OpenDialogAsync<EditContact>(Localizer["AddContact"], parameters.Any() ? parameters : null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Contact> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Contact contact)
        {
            await DialogService.OpenDialogWithOutHeaderAsync<EditContact>(Localizer["Edit Contact"], new Dictionary<string, object> { { "Oid", contact.Oid } }, 80, 80);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Contact contact)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await ContactApiService.Delete(oid:contact.Oid);

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