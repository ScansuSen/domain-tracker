using DomainTracker.Core.Constants;
using DomainTracker.DTOs.Auth;
using FluentValidation;

namespace DomainTracker.Business.Validation
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(Messages.UsernameRequired)
                .Length(3, 100).WithMessage(Messages.UsernameLengthInvalid)
                .Matches("^[a-zA-Z0-9_.-]+$").WithMessage(Messages.UsernameFormatInvalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Messages.PasswordRequired)
                .MinimumLength(6).WithMessage(Messages.PasswordTooShort)
                .MaximumLength(100).WithMessage(Messages.PasswordTooLong);
        }
    }
}
