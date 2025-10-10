using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Client.Components.CustomComponents;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class InventoryItemDropDownDataGrid : VanigamAccountingDropDownAddDataGrid<InventoryItem, EditInventoryItem>
{
    [Inject] InventoryItemApiService InventoryItemApiService { get; set; }
    public InventoryItemDropDownDataGrid()
    {
        Name = "cbx_InventoryItemId";
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ApiService = InventoryItemApiService;
        Width = 35;
        Height = 50;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        this.Columns = (builder2) =>
        {
            builder2.OpenComponent<RadzenDropDownDataGridColumn>(0);
            builder2.AddAttribute(1, "Property", nameof(InventoryItem.Name));
            builder2.AddAttribute(2, "Title", "Name");
            builder2.CloseComponent();

            builder2.OpenComponent<RadzenDropDownDataGridColumn>(1);
            builder2.AddAttribute(1, "Property", nameof(InventoryItem.QuantityOnHand));
            builder2.AddAttribute(2, "Title", "QuantityOnHand");
            builder2.CloseComponent();
        };
    }

    protected override string GetCustomFilter(LoadDataArgs args)
    {
        return $"{nameof(Item.SKU).GetContainsFilter(args.Filter)} or {nameof(Item.Name).GetContainsFilter(args.Filter)}";
    }
}
public class ItemDropDownDataGrid : VanigamAccountingDropDownAddDataGrid<Item, EditInventoryItem>
{
    [Inject] ItemApiService ItemApiService { get; set; }
    public ItemDropDownDataGrid()
    {
        Name = "cbx_ItemId";
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ApiService = ItemApiService;
        Width = 35;
        Height = 50;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        this.Columns = (builder2) =>
        {
            builder2.OpenComponent<RadzenDropDownDataGridColumn>(0);
            builder2.AddAttribute(1, "Property", nameof(Item.Name));
            builder2.AddAttribute(2, "Title", "Name");
            builder2.CloseComponent();

            builder2.OpenComponent<RadzenDropDownDataGridColumn>(1);
            builder2.AddAttribute(1, "Property", nameof(Item.Type));
            builder2.AddAttribute(2, "Title", "Type");
            builder2.CloseComponent();
        };
    }

    protected override string GetCustomFilter(LoadDataArgs args)
    {
        return $"{nameof(Item.SKU).GetContainsFilter(args.Filter)} or {nameof(Item.Name).GetContainsFilter(args.Filter)}";
    }
}