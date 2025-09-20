using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class JobStatusEnumDropDown : VanigamSimpleDropDown<JobStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_JobStatus";
        Data = ((JobStatus[])Enum.GetValues(typeof(JobStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}