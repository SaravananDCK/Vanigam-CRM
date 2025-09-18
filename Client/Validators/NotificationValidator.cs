using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class NotificationValidator : AbstractValidator<Notification>
    {
        public NotificationValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Type).NotEmpty().WithMessage(localizer["TypeRequired"]);
        }
    }
}