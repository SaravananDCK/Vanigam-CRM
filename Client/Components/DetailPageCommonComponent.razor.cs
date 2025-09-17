using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Components
{
    public partial class DetailPageCommonComponent
    {
        [Parameter]
        public string TitleText { get; set; }

        [Parameter]
        public Guid CurrentOid { get; set; }

        [Parameter]
        public bool HasChanges { get; set; }

        [Parameter]
        public bool CanEdit { get; set; }        

        [Parameter]
        public DialogService DialogService { get; set; }

        [Parameter]
        public EventCallback<MouseEventArgs> ReloadButtonClick { get; set; }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.CloseDialog(null);
        }
    }
}
