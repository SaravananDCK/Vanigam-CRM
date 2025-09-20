using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Client.Pages.DetailView;

namespace Vanigam.CRM.Client.Pages.ListView
{
    public partial class Customers
    {
        // Selected customer for nested views
        protected Customer? SelectedCustomer;
        protected int SelectedTabIndex = 0;
        protected async Task GridLoadData(LoadDataArgs args)
        {
            try
            {
                var result = await CustomerApiService.Get(filter: GetFilterString(args), orderBy: $"{args.OrderBy}", top: args.Top, skip: args.Skip, count:args.Top != null && args.Skip != null);
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
            return new ODataFilter<Customer>()
                .FilterByAnd(args.Filter)
                .BeginGroup()
                .ContainsOr(u => u.Name, SearchString)
                .ContainsOr(u => u.Industry, SearchString)
                .ContainsOr(u => u.Address, SearchString)
                .ContainsOr(u => u.Email, SearchString)
                .ContainsOr(u => u.Phone, SearchString)
                .EndGroup()
                .Build();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenDialogAsync<EditCustomer>(Localizer["AddCustomer"], null, 80, 80);
            await GridReload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Customer> args)
        {
            await Open(args.Data);
        }

        private async Task Open(Customer customer)
        {
            await DialogService.OpenDialogWithOutHeaderAsync<EditCustomer>(Localizer["EditCustomer"], new Dictionary<string, object> { { "Oid", customer.Oid } }, 100, 100);
            await GridReload();
        }

        protected async Task GridDeleteButtonClick(Customer customer)
        {
            try
            {
                if (await DialogService.Confirm(Localizer["DeleteRecord"]) == true)
                {
                    var deleteResult = await CustomerApiService.Delete(oid:customer.Oid);

                    if (deleteResult != null)
                    {
                        await GridReload();

                        // Clear selection if deleted customer was selected
                        if (SelectedCustomer?.Oid == customer.Oid)
                        {
                            SelectedCustomer = null;
                        }

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

        protected void OnRowSelect(Customer customer)
        {
            SelectedCustomer = customer;
            SelectedTabIndex = 0; // Reset to first tab when selecting a new customer
        }
    }
}