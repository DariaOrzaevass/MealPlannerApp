using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace MealPlanner.Application.Users.RegisterUser;

// FluentValidation validator for RegisterUserCommand
internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithErrorCode("User.EmailRequired")
            .WithMessage("Email address is required.")
            .EmailAddress().WithErrorCode("User.EmailInvalid")
            .WithMessage("Email address is invalid.")
            .MaximumLength(256).WithErrorCode("User.EmailTooLong")
            .WithMessage("Email address cannot be longer than 256 symbols.");

        RuleFor(c => c.Password)
            .NotEmpty().WithErrorCode("User.PasswordRequired")
            .WithMessage("Password is required.")
            .MinimumLength(8).WithErrorCode("User.PasswordTooShort")
            .WithMessage("The password must contain at least 8 symbols.")
            .Matches("[A-Z]").WithErrorCode("User.PasswordNeedsUpper")
            .WithMessage("The password must contain an uppercase letter.")
            .Matches("[a-z]").WithErrorCode("User.PasswordNeedsLower")
            .WithMessage("The password must contain an lowercase letter.")
            .Matches("[0-9]").WithErrorCode("User.PasswordNeedsDigit")
            .WithMessage("The password must contain a digit.");
    }
}
