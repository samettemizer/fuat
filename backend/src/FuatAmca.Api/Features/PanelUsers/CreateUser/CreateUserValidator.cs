using FluentValidation;

namespace FuatAmca.Api.Features.PanelUsers.CreateUser;

internal sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(request => request.FullName)
            .NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MaximumLength(160);

        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(request => request.Password)
            .NotEmpty().WithMessage("Parola zorunludur.")
            .MinimumLength(10).WithMessage("Parola en az 10 karakter olmalıdır.");

        RuleFor(request => request.Roles)
            .NotEmpty().WithMessage("En az bir rol seçilmelidir.");

        RuleFor(request => request.ApartmanIds)
            .NotEmpty().WithMessage("En az bir apartman yetkisi seçilmelidir.");
    }
}
