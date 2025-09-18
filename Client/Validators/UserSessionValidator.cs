using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class UserSessionValidator : AbstractValidator<UserSession>
    {
        public UserSessionValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.UserId).NotEmpty().WithMessage(localizer["UserIdRequired"]);
            RuleFor(c => c.LoginTime).NotEmpty().WithMessage(localizer["LoginTimeRequired"]);
            RuleFor(c => c.LastActivityTime).NotEmpty().WithMessage(localizer["LastActivityTimeRequired"]);
            RuleFor(c => c.LogoutTime).GreaterThan(c => c.LoginTime).When(c => c.LogoutTime.HasValue)
                .WithMessage(localizer["LogoutTimeMustBeAfterLoginTime"]);
            RuleFor(c => c.Latitude).InclusiveBetween(-90, 90).When(c => c.Latitude.HasValue)
                .WithMessage(localizer["LatitudeMustBeBetweenMinus90And90"]);
            RuleFor(c => c.Longitude).InclusiveBetween(-180, 180).When(c => c.Longitude.HasValue)
                .WithMessage(localizer["LongitudeMustBeBetweenMinus180And180"]);
        }
    }
}