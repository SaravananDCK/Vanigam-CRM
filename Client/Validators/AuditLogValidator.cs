using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class AuditLogValidator : AbstractValidator<AuditLog>
    {
        public AuditLogValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.EntityName).NotEmpty().WithMessage(localizer["EntityNameRequired"]);
            RuleFor(c => c.EntityId).NotEmpty().WithMessage(localizer["EntityIdRequired"]);
            RuleFor(c => c.Action).NotEmpty().WithMessage(localizer["ActionRequired"]);
        }
    }
}