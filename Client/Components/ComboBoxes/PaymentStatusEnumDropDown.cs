using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Components.ComboBoxes;

public class PaymentStatusEnumDropDown : VanigamSimpleDropDown<PaymentStatus>
{
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        Name = "txt_PaymentStatus";
        Data = ((PaymentStatus[])Enum.GetValues(typeof(PaymentStatus))).ToList();
        await base.SetParametersAsync(parameters);
    }
}