using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;
public class AccountTypeEnumDropDown : VanigamSimpleDropDown<AccountType>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_AccountType";
        Data = ((AccountType[])Enum.GetValues(typeof(AccountType))).ToList();
        await base.SetParametersAsync(parameters);
    }
}