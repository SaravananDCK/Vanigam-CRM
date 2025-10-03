using FluentValidation;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class LedgerAccountValidator : AbstractValidator<LedgerAccount>
    {
        public LedgerAccountValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

            RuleFor(x => x.AccountType)
                .IsInEnum().WithMessage("Valid Account Type is required");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Valid email address is required")
                .MaximumLength(200).WithMessage("Email cannot exceed 200 characters");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters");

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

            RuleFor(x => x.City)
                .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

            RuleFor(x => x.State)
                .MaximumLength(100).WithMessage("State cannot exceed 100 characters");

            RuleFor(x => x.PostalCode)
                .MaximumLength(20).WithMessage("Postal Code cannot exceed 20 characters");

            RuleFor(x => x.Country)
                .MaximumLength(100).WithMessage("Country cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.CreditLimit)
                .GreaterThanOrEqualTo(0).WithMessage("Credit Limit must be greater than or equal to 0");
        }
    }
}
