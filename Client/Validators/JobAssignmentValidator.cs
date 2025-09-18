using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class JobAssignmentValidator : AbstractValidator<JobAssignment>
    {
        public JobAssignmentValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.JobId).NotEmpty().WithMessage(localizer["JobRequired"]);
            RuleFor(c => c.AcceptedAt).GreaterThan(c => c.AssignedAt).When(c => c.AcceptedAt.HasValue && c.AssignedAt.HasValue)
                .WithMessage(localizer["AcceptedAtMustBeAfterAssignedAt"]);
        }
    }
}