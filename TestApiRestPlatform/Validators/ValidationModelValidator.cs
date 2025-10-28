using FluentValidation;
using TestApiRestPlatform.Models;
using System.Text.RegularExpressions;

namespace TestApiRestPlatform.Validators;

public class ValidationModelValidator : AbstractValidator<ValidationModel>
{
    public ValidationModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(3, 20).WithMessage("Name must be between 3 and 20 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.PhoneNumber)
            .Must(BeAValidPhoneNumber).WithMessage("Phone number must be a valid format (e.g., +1-234-567-8900, (234) 567-8900, 234-567-8900)")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }

    private bool BeAValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true; // Phone number is optional

        // Phone number pattern that accepts various formats
        // Matches: +1-234-567-8900, (234) 567-8900, 234-567-8900, 234.567.8900, 2345678900, etc.
        var phoneRegex = new Regex(@"^[\+]?[(]?[0-9]{1,4}[)]?[-\s\.]?[(]?[0-9]{1,4}[)]?[-\s\.]?[0-9]{1,4}[-\s\.]?[0-9]{1,9}$");
        return phoneRegex.IsMatch(phoneNumber);
    }
}
