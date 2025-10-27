using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class AddFileDocument
    {
        [Parameter]
        public Guid? LedgerAccountId { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (Oid == Guid.Empty)
                CurrentObject = new();
            else
                CurrentObject = await FileDocumentApiService.GetByOid(oid: Oid);
            await InitEditContext();
        }

        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                CurrentObject.LedgerAccountId = LedgerAccountId;
                var result = await FileDocumentApiService.Create(CurrentObject);
                DialogService.CloseDialog(CurrentObject);
            }
            catch (Exception ex)
            {
                ErrorVisible = true;
            }
            IsBusy = false;
        }
        async Task OnChange(string value, string name)
        {
            if ((name == nameof(CurrentObject.FileName)))
            {
                //CurrentObject.Photo = (value != null ? Convert.FromBase64String(value.Replace("data:image/jpeg;base64,", string.Empty)) : null);
                CurrentObject.Content = value;
                CurrentObject.RefreshFileType();
            }
        }
        protected async Task ReloadButtonClick(MouseEventArgs args)
        {
            HasChanges = false;
            CanEdit = true;

            CurrentObject = await FileDocumentApiService.GetByOid(oid: Oid);
        }
    }
}
