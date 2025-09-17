using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class VanigamAccountingDatePicker<T> : RadzenDatePicker<T>
    {
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            ParseInput = ParseDate;
            Style = "display: block; width: 100%";
            if (typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(DateTimeOffset?))
            {
                ShowTime = true;
                Placeholder = "MM/dd/yyyy h:mm tt";
                DateFormat = "MM/dd/yyyy h:mm tt";
                HourFormat = "12";
            }
            else
            {
                Placeholder = "MM/dd/yyyy";
                DateFormat = "MM/dd/yyyy";
            }
            ValueChanged = EventCallback.Factory.Create<T>(this, CustomChange);
            await base.SetParametersAsync(parameters);
        }

        [Parameter]
        public object UtcDateTime
        {
            get
            {
                if (typeof(T) == typeof(DateTimeOffset))
                {
                   return ((DateTimeOffset)Value).ToUniversalTime();
                }
                else if (typeof(T) == typeof(DateTimeOffset?))
                {
                    return ((DateTimeOffset?)Value).Value.ToUniversalTime();
                }
                return Value as dynamic;
            }
            set
            {
                if (typeof(T) == typeof(DateTimeOffset))
                {
                    Value= ((DateTimeOffset)value).LocalDateTime;
                }
                else if (typeof(T) == typeof(DateTimeOffset?) && value!=null)
                {
                    Value = ((DateTimeOffset?)value).Value.LocalDateTime;
                }
                else
                {
                    Value = value;
                }
                
            }
        }
        [Parameter]
        public EventCallback<T> UtcDateTimeChanged { get; set; }
        [Parameter]
        public Expression<Func<T>> UtcDateTimeExpression { get; set; }
        protected FieldIdentifier UtcDateTimeFieldIdentifier { get; private set; }

        private async Task CustomChange(T value)
        {
            UtcDateTime = value;
            await UtcDateTimeChanged.InvokeAsync(value);
            if (UtcDateTimeExpression != null && string.IsNullOrEmpty(UtcDateTimeFieldIdentifier.FieldName))
            {
                UtcDateTimeFieldIdentifier = FieldIdentifier.Create<T>(UtcDateTimeExpression);
            }
            this.EditContext?.NotifyFieldChanged(this.UtcDateTimeFieldIdentifier);
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
}

