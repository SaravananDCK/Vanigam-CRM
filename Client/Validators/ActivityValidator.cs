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

            RuleFor(c => c.Duration).GreaterThan(0).When(c => c.Duration.HasValue)
                .WithMessage(localizer["DurationMustBePositive"]);

            RuleFor(c => c.Priority).NotEmpty().WithMessage(localizer["PriorityRequired"]);

            RuleFor(c => c.Outcome).MaximumLength(2000).WithMessage(localizer["OutcomeTooLong"]);
        }
    }
}