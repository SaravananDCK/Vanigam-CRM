
using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Client.Components.CustomComponents;
using Vanigam.CRM.Client.Services;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class DocxTemplateDropDownDataGrid : VanigamAccountingDropDownDataGrid<DocxTemplate>
{
    [Inject] DocxTemplateApiService DocxTemplateApiService { get; set; }
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ApiService = DocxTemplateApiService;
    }
    public DocxTemplateDropDownDataGrid()
    {
        TextProperty = nameof(DocxTemplate.Name);
        Name = "cbx_DocxTemplateId";
        SelectedFirstItemByDefault = true;
    }
}
