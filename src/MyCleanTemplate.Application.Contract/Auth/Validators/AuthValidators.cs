using FluentValidation;
using MyCleanTemplate.Application.Contract.Auth.Commands;

namespace MyCleanTemplate.Application.Contract.Auth.Validators;

public class SignInValidator : AbstractValidator<SignInCommand>
{
    public SignInValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class EnableMfaValidator : AbstractValidator<EnableMfaCommand>
{
    public EnableMfaValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.MfaCode).NotEmpty();
    }
}
