using FluentValidation;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanGuncelle;

internal sealed class ApartmanGuncelleValidator : AbstractValidator<ApartmanGuncelleRequest>
{
    public ApartmanGuncelleValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Apartman adı zorunludur.")
            .MaximumLength(160);

        RuleFor(request => request.Address).MaximumLength(400);

        RuleFor(request => request.IsinmaTipi)
            .IsInEnum().WithMessage("Isınma tipi geçersiz.");

        RuleFor(request => request.SonOdemeGunu)
            .InclusiveBetween(1, 28).WithMessage("Son ödeme günü 1 ile 28 arasında olmalıdır.");
    }
}
