using FluentValidation;
using MyCleanTemplate.Application.Contract.Test.Commands;

namespace MyCleanTemplate.Application.Contract.Test.Validators;

public class TestCreateValidator : AbstractValidator<CreateTestCommand>
{
    public TestCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        //RuleFor(x => x.Date).Must(a => a.Hour > 6).WithMessage("Should be after Sunrise!");
    }
}