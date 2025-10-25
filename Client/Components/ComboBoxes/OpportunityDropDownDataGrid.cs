using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Client.Components.CustomComponents;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class OpportunityDropDownDataGrid : VanigamAccountingDropDownAddDataGrid<Opportunity, EditOpportunity>
{
    [Inject] OpportunityApiService OpportunityApiService { get; set; }
    public OpportunityDropDownDataGrid()
    {
        Name = "cbx_OpportunityId";
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ApiService = OpportunityApiService;
        Width = 35;
        Height = 50;

        ValueProperty = nameof(Opportunity.Oid);
        TextProperty = nameof(Opportunity.Title);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        this.Columns = (builder2) =>
        {
            builder2.OpenComponent<RadzenDropDownDataGridColumn>(0);
            builder2.AddAttribute(1, "Property", nameof(Opportunity.Title));
            builder2.AddAttribute(2, "Title", nameof(Opportunity.Title));
            builder2.CloseComponent();
        };
    }

    protected override string GetCustomFilter(LoadDataArgs args)
    {
        return $"{nameof(Item.SKU).GetContainsFilter(args.Filter)} or {nameof(Item.Name).GetContainsFilter(args.Filter)}";
    }
}