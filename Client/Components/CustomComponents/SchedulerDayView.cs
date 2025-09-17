using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class SchedulerDayView : RadzenDayView
    {
        [Parameter] public EventCallback<DateTime> CustomDateChange { get; set; }
        [Parameter] public DateTime CustomDate { get; set; }
        [Parameter] public bool IsCustomDate { get; set; }

        public override string Title
        {
            get
            {
                if(IsCustomDate) 
                    return CustomDate.ToString(Scheduler.Culture.DateTimeFormat.ShortDatePattern);

                return base.Title;
            }
        }

        public override DateTime StartDate
        {
            get
            {
                if (IsCustomDate)
                {
                    var days = (CustomDate.Date - Scheduler.CurrentDate.Date).Days;
                    return Scheduler.CurrentDate.Date.AddDays(days).Add(StartTime);
                }
                return base.StartDate;
            }
        }
        public override DateTime EndDate
        {
            get
            {
                if (IsCustomDate)
                    return Scheduler.CurrentDate.Date.AddDays((CustomDate.Date - Scheduler.CurrentDate.Date).Days).Add(EndTime);
                
                return base.EndDate;
            }
        }
        public override DateTime Next()
        {
            var result = base.Next();
            
            if (IsCustomDate)
                result = CustomDate.Date.AddDays(1);
            
            CustomDateChange.InvokeAsync(result);
            return result;
        }
        public override DateTime Prev()
        {
            var result = base.Prev();
            
            if (IsCustomDate)
                result = CustomDate.Date.AddDays(-1);
            
            CustomDateChange.InvokeAsync(result);
            return result;
        }
    }
}

