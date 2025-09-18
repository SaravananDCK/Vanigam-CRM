using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class FeedbackValidator : AbstractValidator<Feedback>
    {
        public FeedbackValidator(IStringLocalizer localizer)
        {
            RuleFor(c => c.CustomerId).NotEmpty().WithMessage(localizer["CustomerRequired"]);
            RuleFor(c => c.Rating).InclusiveBetween(1, 5).WithMessage(localizer["RatingMustBeBetween1And5"]);
        }
    }
}