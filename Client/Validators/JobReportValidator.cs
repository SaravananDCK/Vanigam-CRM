using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class JobReportValidator : AbstractValidator<JobReport>
    {
        public JobReportValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.JobId).NotEmpty().WithMessage(localizer["JobRequired"]);
        }
    }
}