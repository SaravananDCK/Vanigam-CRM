using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class BankAccountValidator : AbstractValidator<BankAccount>
    {
        public BankAccountValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
            RuleFor(c => c.Code).NotEmpty().WithMessage(localizer["CodeRequired"]);
            RuleFor(c => c.BankName).NotEmpty().WithMessage(localizer["BankNameRequired"]);
            RuleFor(c => c.AccountNumber).NotEmpty().WithMessage(localizer["AccountNumberRequired"]);
            RuleFor(c => c.Email).EmailAddress().When(c => !string.IsNullOrEmpty(c.Email))
                .WithMessage(localizer["EmailInvalid"]);
        }
    }
}
