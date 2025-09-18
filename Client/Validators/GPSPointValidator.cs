using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class GPSPointValidator : AbstractValidator<GPSPoint>
    {
        public GPSPointValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Latitude).InclusiveBetween(-90, 90).WithMessage(localizer["LatitudeMustBeBetweenMinus90And90"]);
            RuleFor(c => c.Longitude).InclusiveBetween(-180, 180).WithMessage(localizer["LongitudeMustBeBetweenMinus180And180"]);
            RuleFor(c => c.RecordedAt).NotEmpty().WithMessage(localizer["RecordedAtRequired"]);
            RuleFor(c => c.Speed).GreaterThanOrEqualTo(0).When(c => c.Speed.HasValue)
                .WithMessage(localizer["SpeedMustBeNonNegative"]);
        }
    }
}