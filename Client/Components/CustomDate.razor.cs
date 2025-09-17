using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Vanigam.CRM.Client.Components;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Components
{
    public partial class CustomDate
    {
        [Inject] private ILocalStorageService LocalStorageService { get; set; }
        [Inject] private DialogService DialogService { get; set; }
        
        [Parameter] public DateTimeOffset FromDate { get; set; }
        [Parameter] public DateTimeOffset ToDate { get; set; }
        [Parameter] public string ComponentName { get; set; }
        [Parameter] public bool ShowDays { get; set; } = true;
        private DateTime MinDate { get; set; }
        private DateTime MaxDate { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            // Set the min and max dates
            MinDate = new DateTime((DateTime.Now.AddDays(-180).Year), (DateTime.Now.AddDays(-180).Month), 1, 0, 0, 0);
            MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
        }

        protected async Task FormSubmit()
        {
            await LocalStorageService.SetItemAsync($"{ComponentName}_{nameof(FromDate)}", FromDate.ToString());
            await LocalStorageService.SetItemAsync($"{ComponentName}_{nameof(ToDate)}", ToDate.ToString());
            DialogService.CloseDialog();
        }

        private async Task FromDateChanged(DateTime arg)
        {
          
            if (ShowDays)
            {
                FromDate = TimeZoneInfo.ConvertTimeToUtc(arg);
                if (ToDate < FromDate) ToDate = FromDate.AddDays(1);
            }
            else
            {
                // Ensure selected date is within the allowed range
                if (arg < MinDate) arg = MinDate;
                else if (arg > MaxDate) arg = MaxDate;

                FromDate = new DateTime(arg.Year, arg.Month, 1);

                // Ensure ToDate is not before FromDate
                if (ToDate < FromDate) ToDate = FromDate;
            }
        }

        private async Task ToDateChanged(DateTime arg)
        {
            if (ShowDays)
            {
                ToDate = TimeZoneInfo.ConvertTimeToUtc(arg);
            }
            else
            {
                // Ensure selected date is within the allowed range
                if (arg < MinDate) arg = MinDate;
                else if (arg > MaxDate) arg = MaxDate;

                ToDate = new DateTime(arg.Year, arg.Month, 1);

                // Ensure FromDate is not before ToDate
                if (ToDate < FromDate) FromDate = ToDate;
            }
        }
        
        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.CloseDialog();
        }
       
        private DateTime? ParseDate(string input)
        {
            string[] formats;
            
            if (ShowDays)
                formats = ["MM-dd-yyyy", "MM/dd/yyyy", "MM-dd-yy", "MM/dd/yy", "MMddyyyy", "MMddyy"];
            else
                formats = ["MM-dd", "MM/dd", "MMdd"];

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(input, format, null, System.Globalization.DateTimeStyles.None, out var result))
                    return result;
            }
            return null;
        }
    }
}
