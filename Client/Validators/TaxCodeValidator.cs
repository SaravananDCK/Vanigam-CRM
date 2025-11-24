using FluentValidation;
using Microsoft.Extensions.Localization;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Validators
{
    public class TaxCodeValidator : AbstractValidator<TaxCode>
    {
        public TaxCodeValidator(IStringLocalizer localizer)
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.TaxType)
                .MaximumLength(50).WithMessage("Tax Type cannot exceed 50 characters");

            RuleFor(x => x.TaxRate)
                .GreaterThanOrEqualTo(0).WithMessage("Tax Rate must be greater than or equal to 0")
                .LessThanOrEqualTo(100).WithMessage("Tax Rate cannot exceed 100%");

            RuleFor(x => x.CGSTRate)
                .GreaterThanOrEqualTo(0).WithMessage("CGST Rate must be greater than or equal to 0")
                .LessThanOrEqualTo(100).WithMessage("CGST Rate cannot exceed 100%");

            RuleFor(x => x.SGSTRate)
                .GreaterThanOrEqualTo(0).WithMessage("SGST Rate must be greater than or equal to 0")
                .LessThanOrEqualTo(100).WithMessage("SGST Rate cannot exceed 100%");

            RuleFor(x => x.IGSTRate)
                .GreaterThanOrEqualTo(0).WithMessage("IGST Rate must be greater than or equal to 0")
                .LessThanOrEqualTo(100).WithMessage("IGST Rate cannot exceed 100%");

            RuleFor(x => x.UTGSTRate)
                .GreaterThanOrEqualTo(0).WithMessage("UTGST Rate must be greater than or equal to 0")
                .LessThanOrEqualTo(100).WithMessage("UTGST Rate cannot exceed 100%");

            RuleFor(x => x.CessRate)
                .GreaterThanOrEqualTo(0).WithMessage("Cess Rate must be greater than or equal to 0")
                .LessThanOrEqualTo(100).WithMessage("Cess Rate cannot exceed 100%");

            RuleFor(x => x.EffectiveTo)
                .GreaterThan(x => x.EffectiveFrom)
                .When(x => x.EffectiveFrom.HasValue && x.EffectiveTo.HasValue)
                .WithMessage("Effective To date must be after Effective From date");

            // Business rule: Either TaxRate should be set OR individual GST components should be set
            RuleFor(x => x)
                .Must(x => x.TaxRate > 0 || x.CGSTRate > 0 || x.SGSTRate > 0 || x.IGSTRate > 0)
                .WithMessage("Either Tax Rate or at least one GST component (CGST/SGST/IGST) must be specified")
                .WithName("TaxRate");
        }
    }
}
