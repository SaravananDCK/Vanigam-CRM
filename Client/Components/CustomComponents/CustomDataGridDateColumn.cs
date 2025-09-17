using Radzen.Blazor;
using Radzen;
using System.Globalization;
using Microsoft.AspNetCore.Components.Rendering;

using System;
using Microsoft.AspNetCore.Components;
using ApexCharts;
using DevExpress.XtraEditors;
using Humanizer;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using DevExpress.Blazor.Internal;
using Npgsql;
namespace Vanigam.CRM.Client.Components.CustomComponents
{
    public class CustomDataGridDateColumn<TItem> : RadzenDataGridColumn<TItem>
    {
        [Parameter] public RenderFragment<RadzenDataGridColumn<TItem>> CustomFilterTemplate { get; set; }
        [Parameter] public bool ShowDates { get; set; } = true;
        public DateTimeOffset? FilterDate { get; set; }
        public DateTime? FilterDateTime { get; set; }

        public RenderFragment<RadzenDataGridColumn<TItem>> DefaultFilterTemplate => context => builder =>
        {
            if (ShowDates)
            {
                // For Days
                builder.OpenComponent<VanigamAccountingDatePicker<DateTimeOffset?>>(0);  // Open the VanigamAccountingDatePicker component
                builder.AddAttribute(1, "Style", "width:100%;");
                builder.AddAttribute(2, "Name", "Filter_StartOn");
                builder.AddAttribute(3, "AllowClear", true);
                builder.AddAttribute(4, "ShowTime", false);
                builder.AddAttribute(5, "Max", DateTime.Now.Date);
                builder.AddAttribute(6, "DateFormat", "yyyy-MM-dd");
                builder.AddAttribute(7, "Placeholder", "yyyy-MM-dd");
                builder.AddAttribute(8, "ShowDays", ShowDates);
                builder.AddAttribute(9, "UtcDateTime", FilterDate);
                builder.AddAttribute(10, "UtcDateTimeChanged", EventCallback.Factory.Create<DateTimeOffset?>(this, value => OnFilterDateChanged(value)));
            }
            else
            {
                // For Months
                builder.OpenComponent<RadzenDatePicker<DateTime>>(0);
                builder.AddAttribute(1, "Style", "width:100%;");
                builder.AddAttribute(2, "Name", "Filter_StartOn");
                builder.AddAttribute(3, "AllowClear", true);
                builder.AddAttribute(4, "ShowTime", false);
                builder.AddAttribute(5, "Max", DateTime.Now.Date);
                builder.AddAttribute(6, "DateFormat", "yyyy-MM");
                builder.AddAttribute(7, "Placeholder", "yyyy-MM");
                builder.AddAttribute(8, "ShowDays", ShowDates);
                builder.AddAttribute(9, "ParseInput", ParseDate);
                builder.AddAttribute(10, "Value", FilterDateTime);
                builder.AddAttribute(11, "CurrentDateChanged", EventCallback.Factory.Create<DateTime>(this, value => OnMonthChanged(value)));
                builder.AddAttribute(12, "ValueChanged", EventCallback.Factory.Create<DateTime>(this, value => OnMonthCleared(value)));
            }
            builder.CloseComponent();
        };

        private DateTime? ParseDate(string input)
        {
            string[] formats;

            if (ShowDates)
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

        private void OnMonthCleared(DateTime value)
        {
            FilterDateTime = null;
            FilterValue = SecondFilterValue = null;
            CloseFilter();
            Grid?.Reload();
        }

        private void OnMonthChanged(DateTime value)
        {
            FilterDateTime = ((DateTime)(object)value);
            FilterValue = new DateTime(FilterDateTime.Value.Year, FilterDateTime.Value.Month, 1);
            FilterOperator = FilterOperator.Equals;
            SetFilterOperator(FilterOperator);
            Grid?.Reload();
        }

        private void OnFilterDateChanged(DateTimeOffset? value)
        {
            FilterDate = value;
            if (value.HasValue)
            {
                var utcDate = TimeZoneInfo.ConvertTimeToUtc(value.Value.Date);
                FilterValue = utcDate;
                SecondFilterValue = utcDate.AddHours(23).AddMinutes(59);
                FilterOperator = FilterOperator.GreaterThanOrEquals;
                SecondFilterOperator = FilterOperator.LessThan;
                SetFilterOperator(FilterOperator);
            }
            else
            {
                FilterValue = SecondFilterValue = null;
                CloseFilter();
            }
            Grid?.Reload();
        }

        private void SetFilterTemplate()
        {
            if (FilterDateTime == null && FilterDate == null)
            {
                ClearFilters();
            }
            FilterTemplate = DefaultFilterTemplate;
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
            SetFilterTemplate();
            await base.SetParametersAsync(parameters);
        }
    }
}




