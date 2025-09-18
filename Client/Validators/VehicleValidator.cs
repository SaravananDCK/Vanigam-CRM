using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class VehicleValidator : AbstractValidator<Vehicle>
    {
        public VehicleValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.RegistrationNumber).NotEmpty().WithMessage(localizer["RegistrationNumberRequired"]);
        }
    }
}