using FluentValidation;

namespace FuatAmca.Api.Features.Authentication.RefreshToken;

internal sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator() =>
        RuleFor(request => request.RefreshToken)
            .NotEmpty().WithMessage("Yenileme tokeni zorunludur.")
            .MaximumLength(200);
}
