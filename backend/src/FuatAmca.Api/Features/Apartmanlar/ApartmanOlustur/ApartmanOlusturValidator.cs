using FluentValidation;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanOlustur;

internal sealed class ApartmanOlusturValidator : AbstractValidator<ApartmanOlusturRequest>
{
    public ApartmanOlusturValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Apartman adı zorunludur.")
            .MaximumLength(160);

        RuleFor(request => request.Address)
            .MaximumLength(400);

        RuleFor(request => request.IsinmaTipi)
            .IsInEnum().WithMessage("Isınma tipi geçersiz.");

        RuleFor(request => request.SonOdemeGunu)
            .InclusiveBetween(1, 28).WithMessage("Son ödeme günü 1 ile 28 arasında olmalıdır.");

        When(request => request.GiderTutarlari is not null, () =>
        {
            RuleFor(request => request.GiderTutarlari!.Aidat)
                .GreaterThanOrEqualTo(0).WithMessage("Aidat tutarı negatif olamaz.");
            RuleFor(request => request.GiderTutarlari!.Yakit)
                .GreaterThanOrEqualTo(0).WithMessage("Yakıt tutarı negatif olamaz.");
            RuleFor(request => request.GiderTutarlari!.BinaTadilati)
                .GreaterThanOrEqualTo(0).WithMessage("Bina tadilatı tutarı negatif olamaz.");
            RuleFor(request => request.GiderTutarlari!.SairGider)
                .GreaterThanOrEqualTo(0).WithMessage("Sair gider tutarı negatif olamaz.");
        });
    }
}
