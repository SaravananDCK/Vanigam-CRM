using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class PaymentMethodEnumDropDown : VanigamSimpleDropDown<PaymentMethod>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_PaymentMethod";
        Data = ((PaymentMethod[])Enum.GetValues(typeof(PaymentMethod))).ToList();
        await base.SetParametersAsync(parameters);
    }
}