using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Microsoft.AspNetCore.Components.Forms;
using Radzen.Blazor;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Components
{
    public class BaseDetailView<T, K> : BaseView<T, K> where T : BaseClass where K : ComponentBase
    {

        [Parameter]
        public Guid Oid { get; set; }

        [CascadingParameter(Name = nameof(IntakeView))]
        public bool IntakeView { get; set; } = false;

        protected T CurrentObject;
        protected bool HasChanges = false;
        protected bool CanEdit = true;
        protected bool ErrorVisible;

        protected IEnumerable<T> Source { get; set; }

        protected CollapseExpand CollapseExpand { get; set; } = CollapseExpand.Collapse;
        [Parameter] public bool ShowButtons { get; set; } = true;

        protected bool ShowNotUniqueAlert = false;
        protected RadzenTemplateForm<T> Form { get; set; }
        protected EditContext EditContext { get; set; }
        protected virtual async Task InitEditContext()
        {
            EditContext = new EditContext(CurrentObject);
            EditContext.OnFieldChanged += EditContext_OnFieldChanged;
        }
        public bool IsEverythingSaved()
        {
            var isModified = Form.EditContext.IsModified();
            Form.EditContext.Validate();
            return !isModified;
        }
        public bool IsValid()
        {
            return EditContext.Validate();
            
        }
        protected void EditContext_OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            StateHasChanged();
            //Console.WriteLine($"Field {e.FieldIdentifier.FieldName} has changed");
        }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.CloseDialog();
        }

        protected void Change(CollapseExpand text)
        {
            CollapseExpand = text;
        }


        protected DateTime? ParseDate(string input)
        {
            string[] formats = ["MM-dd-yyyy", "MM/dd/yyyy", "MM-dd-yy", "MM/dd/yy", "MMddyyyy", "MMddyy", "MM-dd", "MM/dd", "MMdd"];

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(input, format, null, System.Globalization.DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }

            return null;
        }

    }

    public class PatientRelatedDetailView<T, K> : BaseDetailView<T, K> where T : BaseClass where K : ComponentBase
    {
        [Parameter] public Guid? PatientId { get; set; }
        [Parameter] public Guid? EncounterId { get; set; }
        [Parameter] public bool ShowPatient { get; set; } = true;
    }

    public class SurveyRelatedDetailView<T, K> : BaseDetailView<T, K> where T : BaseClass where K : ComponentBase
    {
        [Parameter] public Guid SurveyTemplateId { get; set; }
        [Parameter] public Guid SurveyCategoryTemplateId { get; set; }
        [Parameter] public Guid SurveyQuestionTemplateId { get; set; }
        [Parameter] public Guid? SurveyGroupTemplateId { get; set; }
        [Parameter] public Guid? AnswerTemplateId { get; set; }
        [Parameter] public Guid? QuestionTemplateId { get; set; }
        [Parameter] public bool AutoDialogClose { get; set; } = false;
    }

    public enum CollapseExpand
    {
        Collapse,
        Expand
    }
}

