using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class TimeSheetValidator : AbstractValidator<TimeSheet>
    {
        public TimeSheetValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.TechnicianId).NotEmpty().WithMessage(localizer["TechnicianRequired"]);
            RuleFor(c => c.JobId).NotEmpty().WithMessage(localizer["JobRequired"]);
            RuleFor(c => c.StartAt).NotEmpty().WithMessage(localizer["StartTimeRequired"]);
            RuleFor(c => c.EndAt).NotEmpty().WithMessage(localizer["EndTimeRequired"])
                .GreaterThan(c => c.StartAt).WithMessage(localizer["EndTimeMustBeAfterStartTime"]);
        }
    }
}