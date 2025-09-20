using DevExpress.Blazor.RichEdit;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Helpers;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages.DetailView
{
    public partial class EditDocxTemplate
    {
        private bool DocumentLoaded { get; set; } = false;
        private DxRichEdit DxRichEdit { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (Oid == Guid.Empty)
                CurrentObject = new();
            else
                CurrentObject = await DocxTemplateApiService.GetByOid(oid: Oid);

            await InitEditContext();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!DocumentLoaded)
            {
                if (CurrentObject!=null && !string.IsNullOrEmpty(CurrentObject.Content))
                {
                    DocumentLoaded = true;
                    await DxRichEdit.LoadDocumentAsync(Convert.FromBase64String(CurrentObject.Content), DocumentFormat.OpenXml);
                }
            }
        }
        protected async Task FormSubmit()
        {
            IsBusy = true;
            try
            {
                CurrentObject.Content = Convert.ToBase64String(await DxRichEdit.ExportDocumentAsync(DocumentFormat.OpenXml));
                if (Oid == Guid.Empty)
                {
                    CurrentObject = await DocxTemplateApiService.Create(CurrentObject);
                }
                else
                {
                    var result = await DocxTemplateApiService.Update(oid: Oid, CurrentObject);
                    if (result.IsPreconditionFailed())
                    {
                        HasChanges = true;
                        CanEdit = false;
                        return;
                    }
                }
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = Localizer["SavedSuccessfully!"] });
                DialogService.CloseDialog(CurrentObject);
            }
            catch (Exception ex)
            {
                ErrorVisible = true;
            }
            IsBusy = false;
        }

        
        protected async Task ReloadButtonClick(MouseEventArgs args)
        {
            HasChanges = false;
            CanEdit = true;
            CurrentObject = await DocxTemplateApiService.GetByOid(oid: Oid);
        }

        protected override async Task SaveAndStayInEdit()
        {
            await FormSubmit();
            // After successful save, switch back to read-only mode
            if (!ErrorVisible && !ShowNotUniqueAlert)
            {
                IsReadOnlyMode = true;
                StateHasChanged();
            }
        }

        private async Task OnDocumentContentChanged(byte[] arg)
        {
            EditContext.NotifyFieldChanged(EditContext.Field(nameof(DocumentTemplate.Content)));
            await FormSubmit();
        }
    }
}

