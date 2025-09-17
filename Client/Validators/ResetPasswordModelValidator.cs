using FluentValidation;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Models;
using Microsoft.Extensions.Localization;

namespace Vanigam.CRM.Client.Validators
{
    public class ResetPasswordModelValidator : AbstractValidator<ResetPasswordModel>
    {
        public ResetPasswordModelValidator(IStringLocalizer localizer)
        {
            RuleFor(r => r.Password).NotEmpty().WithMessage(localizer["NewPasswordRequired"]);
            RuleFor(r => r.ConfirmPassword).NotEmpty().WithMessage(localizer["ConfirmPasswordRequired"]);
            RuleFor(u => u.ConfirmPassword).Equal(u => u.Password).WithMessage(localizer["PasswordMatchAlert"]);
        }
    }
}

