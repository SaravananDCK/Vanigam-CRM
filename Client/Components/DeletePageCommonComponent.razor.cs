using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace Vanigam.CRM.Client.Components;

public partial class DeletePageCommonComponent<T>
{
    [Parameter] public NavigationManager NavigationManager { get; set; }
    [Parameter] public T UsingObject { get; set; }
    [Parameter] public EventCallback<T> GridDeleteButtonClick { get; set; }
    [Parameter] public Shade Shade { get; set; } = Radzen.Shade.Lighter;

    public async Task SelectedValue(MouseEventArgs Args, T currentObject)
    {
        await GridDeleteButtonClick.InvokeAsync(currentObject);
    }
}
