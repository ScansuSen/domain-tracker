using DomainTracker.Core.Constants;
using DomainTracker.DTOs.Auth;
using FluentValidation;

namespace DomainTracker.Business.Validation
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(Messages.UsernameRequired);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Messages.PasswordRequired);
        }
    }
}
