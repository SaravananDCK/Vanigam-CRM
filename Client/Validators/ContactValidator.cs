using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class ContactValidator : AbstractValidator<Contact>
    {
        public ContactValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Email).EmailAddress().When(c => !string.IsNullOrEmpty(c.Email))
                .WithMessage(localizer["EmailInvalid"]);

            RuleFor(c => c.FirstName).NotEmpty().WithMessage(localizer["FirstNameRequired"]);

            RuleFor(c => c.LastName).NotEmpty().WithMessage(localizer["LastNameRequired"]);

            RuleFor(c => c.CustomerId).NotEmpty().WithMessage(localizer["CustomerRequired"]);

            RuleFor(c => c.JobTitle).MaximumLength(100).WithMessage(localizer["JobTitleTooLong"]);

            RuleFor(c => c.Department).MaximumLength(100).WithMessage(localizer["DepartmentTooLong"]);

            RuleFor(c => c.Phone).Matches(@"^[\+]?[1-9][\d]{0,15}$").When(c => !string.IsNullOrEmpty(c.Phone))
                .WithMessage(localizer["PhoneInvalid"]);

            RuleFor(c => c.Mobile).Matches(@"^[\+]?[1-9][\d]{0,15}$").When(c => !string.IsNullOrEmpty(c.Mobile))
                .WithMessage(localizer["MobileInvalid"]);

            RuleFor(c => c.LinkedInProfile).Must(BeValidUrl).When(c => !string.IsNullOrEmpty(c.LinkedInProfile))
                .WithMessage(localizer["LinkedInProfileInvalid"]);

            RuleFor(c => c.Address).MaximumLength(500).WithMessage(localizer["AddressTooLong"]);

            RuleFor(c => c.City).MaximumLength(100).WithMessage(localizer["CityTooLong"]);

            RuleFor(c => c.State).MaximumLength(100).WithMessage(localizer["StateTooLong"]);

            RuleFor(c => c.PostalCode).MaximumLength(20).WithMessage(localizer["PostalCodeTooLong"]);

            RuleFor(c => c.Country).MaximumLength(100).WithMessage(localizer["CountryTooLong"]);

            RuleFor(c => c.Notes).MaximumLength(2000).WithMessage(localizer["NotesTooLong"]);
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}