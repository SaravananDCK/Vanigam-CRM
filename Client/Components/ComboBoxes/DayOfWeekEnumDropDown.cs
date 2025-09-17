using Microsoft.AspNetCore.Components;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public partial class DayOfWeekEnumDropDown : ComponentBase
{
    [Parameter] public DayOfWeek? Value { get; set; }
    [Parameter] public EventCallback<DayOfWeek?> ValueChanged { get; set; }
    [Parameter] public string Name { get; set; } = string.Empty;

    private List<DayOfWeekOption> Options { get; set; } = new();

    protected override void OnInitialized()
    {
        Options = new List<DayOfWeekOption>
        {
            new() { Value = DayOfWeek.Monday, Text = "Monday" },
            new() { Value = DayOfWeek.Tuesday, Text = "Tuesday" },
            new() { Value = DayOfWeek.Wednesday, Text = "Wednesday" },
            new() { Value = DayOfWeek.Thursday, Text = "Thursday" },
            new() { Value = DayOfWeek.Friday, Text = "Friday" },
            new() { Value = DayOfWeek.Saturday, Text = "Saturday" },
            new() { Value = DayOfWeek.Sunday, Text = "Sunday" }
        };
    }

    private async Task OnValueChanged(DayOfWeekOption? selectedOption)
    {
        Value = selectedOption?.Value;
        await ValueChanged.InvokeAsync(Value);
    }

    private DayOfWeekOption? SelectedOption
    {
        get => Value.HasValue ? Options.FirstOrDefault(o => o.Value == Value.Value) : null;
        set => _ = OnValueChanged(value);
    }

    private class DayOfWeekOption
    {
        public DayOfWeek Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
