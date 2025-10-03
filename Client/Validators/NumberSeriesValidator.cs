using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class NumberSeriesValidator : AbstractValidator<NumberSeries>
    {
        public NumberSeriesValidator(IStringLocalizer localizer)
        {
            RuleFor(ns => ns.EntityType)
                .NotEmpty()
                .WithMessage(localizer["EntityTypeRequired"])
                .MaximumLength(100)
                .WithMessage(localizer["EntityTypeMaxLength"]);

            RuleFor(ns => ns.Prefix)
                .MaximumLength(20)
                .WithMessage(localizer["PrefixMaxLength"]);

            RuleFor(ns => ns.Suffix)
                .MaximumLength(20)
                .WithMessage(localizer["SuffixMaxLength"]);

            RuleFor(ns => ns.StartNo)
                .GreaterThan(0)
                .WithMessage(localizer["StartNoMustBeGreaterThanZero"]);

            RuleFor(ns => ns.CurrentNo)
                .GreaterThan(0)
                .WithMessage(localizer["CurrentNoMustBeGreaterThanZero"]);

            RuleFor(ns => ns.PaddingLength)
                .InclusiveBetween(1, 10)
                .WithMessage(localizer["PaddingLengthRange"]);
        }
    }
}
