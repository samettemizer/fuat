using FluentValidation;

namespace FuatAmca.Api.Features.PanelUsers.ResetPassword;

internal sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(request => request.NewPassword)
            .NotEmpty().WithMessage("Parola zorunludur.")
            .MinimumLength(10).WithMessage("Parola en az 10 karakter olmalıdır.");
    }
}
