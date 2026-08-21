using DomainTracker.Core.Constants;
using DomainTracker.DTOs.Auth;
using FluentValidation;

namespace DomainTracker.Business.Validation
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
    {
        private const int MinimumPasswordLength = 3;

        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(Messages.UsernameRequired)
                .Length(3, 100).WithMessage(Messages.UsernameLengthInvalid)
                .Matches("^[a-zA-Z0-9_.-]+$").WithMessage(Messages.UsernameFormatInvalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Messages.PasswordRequired)
                .MinimumLength(MinimumPasswordLength).WithMessage(Messages.PasswordTooShort(MinimumPasswordLength))
                .MaximumLength(100).WithMessage(Messages.PasswordTooLong);
        }
    }
}
