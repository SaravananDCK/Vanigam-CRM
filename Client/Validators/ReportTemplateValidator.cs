using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class ReportTemplateValidator : AbstractValidator<ReportTemplate>
    {
        public ReportTemplateValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage(localizer["NameRequired"]);
            RuleFor(c => c.Content).NotEmpty().WithMessage(localizer["ContentRequired"]);
            RuleFor(c => c.FileName).NotEmpty().WithMessage(localizer["FileNameRequired"]);
        }
    }
}