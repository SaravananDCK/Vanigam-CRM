using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class CustomFieldValidator : AbstractValidator<CustomField>
    {
        public CustomFieldValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.EntityName).NotEmpty().WithMessage(localizer["EntityNameRequired"]);
            RuleFor(c => c.FieldName).NotEmpty().WithMessage(localizer["FieldNameRequired"]);
            RuleFor(c => c.FieldType).NotEmpty().WithMessage(localizer["FieldTypeRequired"]);
        }
    }
}