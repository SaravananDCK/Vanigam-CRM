using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class ContactTypeEnumDropDown : VanigamSimpleDropDown<ContactType>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_ContactType";
        Data = ((ContactType[])Enum.GetValues(typeof(ContactType))).ToList();
        await base.SetParametersAsync(parameters);
    }
}