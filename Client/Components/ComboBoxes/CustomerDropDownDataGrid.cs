using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Client.Components.CustomComponents;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class CustomerDropDownDataGrid : VanigamAccountingDropDownAddDataGrid<Customer, EditCustomer>
{
    [Inject] CustomerApiService CustomerApiService { get; set; }
    public CustomerDropDownDataGrid()
    {
        Name = "cbx_CustomerId";
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ApiService = CustomerApiService;
        Width = 35;
        Height = 50;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        this.Columns = (builder2) =>
        {
            builder2.OpenComponent<RadzenDropDownDataGridColumn>(0);
            builder2.AddAttribute(1, "Property", nameof(Customer.Code));
            builder2.AddAttribute(2, "Title", "Code");
            builder2.CloseComponent();

            builder2.OpenComponent<RadzenDropDownDataGridColumn>(1);
            builder2.AddAttribute(1, "Property", nameof(Customer.Name));
            builder2.AddAttribute(2, "Title", "Name");
            builder2.CloseComponent();
        };
    }

    protected override string GetCustomFilter(LoadDataArgs args)
    {
        return $"{nameof(Item.SKU).GetContainsFilter(args.Filter)} or {nameof(Item.Name).GetContainsFilter(args.Filter)}";
    }
}