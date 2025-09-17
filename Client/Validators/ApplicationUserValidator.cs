using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRMValidators
{
    public class ApplicationUserValidator : AbstractValidator<ApplicationUser>
    {
        public ApplicationUserValidator(IStringLocalizer localizer,bool ShowResetPassword)
        {
              if(!ShowResetPassword)
                {
                RuleFor(u => u.Password).NotEmpty().WithMessage(localizer["PasswordReq"]);
                RuleFor(u => u.ConfirmPassword).Equal(u => u.Password).WithMessage(localizer["PasswordMatchAlert"]);
                }
            }
    }
}

