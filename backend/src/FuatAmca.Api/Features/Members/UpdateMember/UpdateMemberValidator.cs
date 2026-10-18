using FluentValidation;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Members.UpdateMember;

internal sealed class UpdateMemberValidator : AbstractValidator<UpdateMemberRequest>
{
    public UpdateMemberValidator()
    {
        RuleFor(request => request.FirstName)
            .NotEmpty().WithMessage("Üye adı zorunludur.")
            .MaximumLength(80);

        RuleFor(request => request.LastName)
            .NotEmpty().WithMessage("Üye soyadı zorunludur.")
            .MaximumLength(80);

        RuleFor(request => request.DaireNo)
            .NotEmpty().WithMessage("Daire no zorunludur.")
            .MaximumLength(10);

        RuleFor(request => request.PhoneNumber)
            .NotEmpty().WithMessage("Cep telefonu zorunludur.")
            .Must(phoneNumber => PhoneNumberRules.Normalize(phoneNumber) is not null)
            .WithMessage("Cep telefonu başında sıfır olmadan 10 haneli olmalıdır.");

        RuleFor(request => request.SakinTipi)
            .IsInEnum().WithMessage("Sakin tipi geçersiz.");

        RuleFor(request => request.EvSahibiFullName)
            .NotEmpty().WithMessage("Kiracı kaydında ev sahibi bilgisi zorunludur.")
            .When(request => request.SakinTipi == SakinTipi.Kiraci);

        RuleFor(request => request.EvSahibiFullName)
            .MaximumLength(160);

        RuleFor(request => request.EvSahibiPhoneNumber)
            .Must(phoneNumber => PhoneNumberRules.Normalize(phoneNumber) is not null)
            .When(request => !string.IsNullOrWhiteSpace(request.EvSahibiPhoneNumber))
            .WithMessage("Ev sahibi cep telefonu başında sıfır olmadan 10 haneli olmalıdır.");

        RuleFor(request => request.Email)
            .EmailAddress().WithMessage("E-posta adresi geçersiz.")
            .MaximumLength(256)
            .When(request => !string.IsNullOrWhiteSpace(request.Email));

        RuleFor(request => request.Notes)
            .MaximumLength(1000);
    }
}
