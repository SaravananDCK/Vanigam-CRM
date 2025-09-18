using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class FileDocumentValidator : AbstractValidator<FileDocument>
    {
        public FileDocumentValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.FileName).NotEmpty().WithMessage(localizer["FileNameRequired"]);
            RuleFor(c => c.Content).NotEmpty().WithMessage(localizer["ContentRequired"]);
        }
    }
}