using DomainTracker.Core.Constants;
using FluentValidation;

namespace DomainTracker.Business.Validation
{
    public class DomainNameValidator : AbstractValidator<string>
    {
        public DomainNameValidator()
        {
            RuleFor(name => name)
                .NotEmpty().WithMessage(Messages.DomainNameRequired)
                .MaximumLength(DomainNameRules.MaxLength).WithMessage(Messages.DomainNameTooLong(DomainNameRules.MaxLength))
                .Matches(DomainNameRules.Pattern).WithMessage(Messages.DomainNameFormatInvalid)
                .WithName("Name");
        }
    }
}
