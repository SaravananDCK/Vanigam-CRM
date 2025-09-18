using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class ActivityValidator : AbstractValidator<Activity>
    {
        public ActivityValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Type).NotEmpty().WithMessage(localizer["TypeRequired"]);
            RuleFor(c => c.Notes).NotEmpty().WithMessage(localizer["NotesRequired"]);
            RuleFor(c => c.Date).NotEmpty().WithMessage(localizer["DateRequired"]);
        }
    }
}