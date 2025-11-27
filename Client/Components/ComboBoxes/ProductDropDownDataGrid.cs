using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Client.Components.CustomComponents;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class ProductDropDownDataGrid : VanigamAccountingDropDownAddDataGrid<Product, EditProduct>
{
    [Inject] ProductApiService ProductApiService { get; set; }
    public ProductDropDownDataGrid()
    {
        Name = "cbx_ProductId";
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ApiService = ProductApiService;
        Width = 35;
        Height = 50;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        this.Columns = (builder2) =>
        {
            builder2.OpenComponent<RadzenDropDownDataGridColumn>(0);
            builder2.AddAttribute(1, "Property", nameof(Product.Name));
            builder2.AddAttribute(2, "Title", "Name");
            builder2.CloseComponent();

            builder2.OpenComponent<RadzenDropDownDataGridColumn>(1);
            builder2.AddAttribute(1, "Property", nameof(Product.UnitPrice));
            builder2.AddAttribute(2, "Title", "Unit Price");
            builder2.CloseComponent();
        };
    }

    protected override string GetCustomFilter(LoadDataArgs args)
    {
        return $"{nameof(Item.SKU).GetContainsFilter(args.Filter)} or {nameof(Item.Name).GetContainsFilter(args.Filter)}";
    }
}