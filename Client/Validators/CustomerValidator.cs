using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
            RuleFor(c => c.Email).EmailAddress().When(c => !string.IsNullOrEmpty(c.Email))
                .WithMessage(localizer["EmailInvalid"]);
            RuleFor(c => c.Website).Must(BeValidUrl).When(c => !string.IsNullOrEmpty(c.Website))
                .WithMessage(localizer["WebsiteInvalid"]);
            RuleFor(c => c.AnnualRevenue).GreaterThanOrEqualTo(0).When(c => c.AnnualRevenue.HasValue)
                .WithMessage(localizer["AnnualRevenueMustBeNonNegative"]);
            RuleFor(c => c.EmployeeCount).GreaterThanOrEqualTo(0).When(c => c.EmployeeCount.HasValue)
                .WithMessage(localizer["EmployeeCountMustBeNonNegative"]);
            RuleFor(c => c.PostalCode).Matches(@"^\d{6}$").When(c => !string.IsNullOrEmpty(c.PostalCode))
                .WithMessage(localizer["PostalCodeMustBe6Digits"]);
        }

        private static bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}