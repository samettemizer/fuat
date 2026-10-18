using FluentValidation;

namespace FuatAmca.Api.Features.Authentication.Logout;

internal sealed class LogoutValidator : AbstractValidator<LogoutRequest>
{
    public LogoutValidator() =>
        RuleFor(request => request.RefreshToken)
            .MaximumLength(200);
}
