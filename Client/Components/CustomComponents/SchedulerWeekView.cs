using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;

namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class SchedulerWeekView : RadzenWeekView
    {
        [Parameter] public EventCallback<DateTime> CustomDateChange { get; set; }
        [Parameter] public DateTime CustomDate { get; set; }
        [Parameter] public bool IsCustomDate { get; set; }

        public override DateTime StartDate
        {
            get
            {
                if (IsCustomDate)
                    return Scheduler.CurrentDate.Date.AddDays((CustomDate.Date - Scheduler.CurrentDate.Date).Days).StartOfWeek();
                
                return base.StartDate;
            }
        }
        public override DateTime EndDate
        {
            get
            {
                if (IsCustomDate)
                    return Scheduler.CurrentDate.Date.AddDays((CustomDate.Date - Scheduler.CurrentDate.Date).Days).EndOfWeek().AddDays(1); 
                
                return base.EndDate;
            }
        }
        public override DateTime Next()
        {
            var result = base.Next();
            if (IsCustomDate)
                result = CustomDate.Date.AddDays(7);
            CustomDateChange.InvokeAsync(result);
            return result;
        }
        public override DateTime Prev()
        {
            var result = base.Prev();
            if (IsCustomDate)
                result = CustomDate.Date.AddDays(-7);
            CustomDateChange.InvokeAsync(result);
            return result;
        }
    }
}

