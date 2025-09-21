using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class ActivityValidator : AbstractValidator<Activity>
    {
        public ActivityValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Subject).NotEmpty().WithMessage(localizer["SubjectRequired"]);
            RuleFor(c => c.Type).NotEmpty().WithMessage(localizer["TypeRequired"]);
            RuleFor(c => c.Status).NotEmpty().WithMessage(localizer["StatusRequired"]);
            RuleFor(c => c.ActivityDate).NotEmpty().WithMessage(localizer["ActivityDateRequired"]);
            RuleFor(c => c.Description).MaximumLength(2000).WithMessage(localizer["DescriptionTooLong"]);
            RuleFor(c => c.Notes).MaximumLength(2000).WithMessage(localizer["NotesTooLong"]);
        }
    }
}