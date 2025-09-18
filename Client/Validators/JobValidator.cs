using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class JobValidator : AbstractValidator<Job>
    {
        public JobValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Title).NotEmpty().WithMessage(localizer["TitleRequired"]);
        }
    }
}