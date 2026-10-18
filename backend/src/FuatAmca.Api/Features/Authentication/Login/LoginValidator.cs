using FluentValidation;

namespace FuatAmca.Api.Features.Authentication.Login;

internal sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(256);

        RuleFor(request => request.Password)
            .NotEmpty().WithMessage("Parola zorunludur.")
            .MaximumLength(128);
    }
}
