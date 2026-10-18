using FluentValidation;

namespace FuatAmca.Api.Features.PanelUsers.UpdateUser;

internal sealed class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(request => request.FullName)
            .NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MaximumLength(160);

        RuleFor(request => request.Roles)
            .NotEmpty().WithMessage("En az bir rol seçilmelidir.");

        RuleFor(request => request.ApartmanIds)
            .NotEmpty().WithMessage("En az bir apartman yetkisi seçilmelidir.");
    }
}
