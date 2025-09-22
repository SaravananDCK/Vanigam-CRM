using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class ContactStatusEnumDropDown : VanigamSimpleDropDown<ContactStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_ContactStatus";
        Data = ((ContactStatus[])Enum.GetValues(typeof(ContactStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}