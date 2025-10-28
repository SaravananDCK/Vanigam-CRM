using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class VendorValidator : AbstractValidator<Vendor>
    {
        public VendorValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
            RuleFor(c => c.Email).EmailAddress().When(c => !string.IsNullOrEmpty(c.Email))
                .WithMessage(localizer["EmailInvalid"]);
            RuleFor(c => c.Website).Must(BeValidUrl).When(c => !string.IsNullOrEmpty(c.Website))
                .WithMessage(localizer["WebsiteInvalid"]);
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
