using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class AttachmentValidator : AbstractValidator<Attachment>
    {
        public AttachmentValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.FileName).NotEmpty().WithMessage(localizer["FileNameRequired"]);
            RuleFor(c => c.ContentType).NotEmpty().WithMessage(localizer["ContentTypeRequired"]);
        }
    }
}