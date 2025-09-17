using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class ChangePasswordValidator : AbstractValidator<ApplicationUser>
    {
        public ChangePasswordValidator(IStringLocalizer localizer)
        {

        }
    }
}

